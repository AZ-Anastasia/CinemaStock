import asyncio
import json
import locale
import os
from pathlib import Path
import time
from types import MappingProxyType
from typing import Type, TypeVar
import typing
import allure
import httpx
from pydantic import RootModel, TypeAdapter
import pytest
from dotenv import load_dotenv
import xml.etree.ElementTree as ET
import pytest_asyncio
from api.api_clients.base_client import AsyncBaseClient
from api.models.base_models import AppBaseModel

from api.models.requests.login_request import LoginRequest

# Загрузка переменных из файла .env в окружение ОС
load_dotenv()

@pytest.fixture(scope="session")
def browser_context_args(browser_context_args):
    return {**browser_context_args, "ignore_https_errors": True}


# region Allure Screenshot

class ScreenshotService:
    def __init__(self, page):
        self._page = page

    def capture(self, name="Allure Screenshot"):
        try:
            self._page.wait_for_load_state("networkidle", timeout=3000)
        except Exception:
            pass

        allure.attach(
            self._page.screenshot(full_page=True),
            name=name,
            attachment_type=allure.attachment_type.PNG,
        )


@pytest.fixture(scope="function")
def screenshot_service(page):
    return ScreenshotService(page)


# Хук для автоматического скрина при падении теста
@pytest.hookimpl(tryfirst=True, hookwrapper=True)
def pytest_runtest_makereport(item, call):
    # "Точка паузы и передачи управления после выполнения теста"
    outcome = yield
    rep = outcome.get_result()
    if rep.failed and call.when == "call":
        if "page" in item.fixturenames:
            page = item.funcargs["page"]
            allure.attach(
                page.screenshot(full_page=True),
                name="Скриншот при падении теста",
                attachment_type=allure.attachment_type.PNG,
            )

# endregion


# region Test config

class TestConfig:
    # Игнорировать этот класс при поиске тестов
    __test__ = False

    def __init__(self):
        self.user_name = os.getenv("TEST_USER_LOGIN")
        self.user_email = os.getenv("TEST_USER_EMAIL")
        self.user_password = os.getenv("TEST_USER_PASSWORD")

        try:
            # getlocale() -> ('ru_RU', 'UTF-8')
            sys_lang = locale.getlocale()[0]  # ru_RU или en_US
            default_lang = "ru" if sys_lang and "ru" in sys_lang.lower() else "en"
        except Exception:
            default_lang = "en"

        self.lang = os.getenv("TEST_LANG", default_lang)

    def generate_random_user(self):
        timestamp = int(time.time())
        return {
            "email": f"autotest_{timestamp}@gmail.com",
            "username": f"user_{timestamp}",
            "password": self.user_password,
        }


# Декоратор
# session - выполнится один раз за запуск всех тестов (за всю тестовую сессию)
@pytest.fixture(scope="session")
def config():
    return TestConfig()

# endregion


# region Загрузка тестовых данных /tests/data

_PROJECT_ROOT = Path.cwd().parent
_PROJECT_TESTS_DIR = Path.cwd()
_PATH_TO_TEST_DATA = _PROJECT_TESTS_DIR / "tests" / "data"
_PATH_TO_RESOURCES = _PROJECT_ROOT / "CinemaStock.Shared" / "Resources"
_RU_ERRORS_RESX_PATH = _PATH_TO_RESOURCES / "UserErrors" / "UserErrors.ru.resx"
_EN_ERRORS_RESX_PATH = _PATH_TO_RESOURCES / "UserErrors" / "UserErrors.resx"


@pytest.fixture(scope="session")
def load_json_file():
    cache = {}

    def _load(file_name):
        if file_name not in cache:
            file_path = _PATH_TO_TEST_DATA / file_name
            with open(file_path, "r", encoding="utf-8") as f:
                cache[file_name] = json.load(f)
        return cache[file_name]

    return _load


@pytest.fixture(scope="function")
def test_data(request, config, load_json_file, load_resx_strings):
    """
    Универсальный сборщик данных для любой страницы.
    Возвращает: словарь со всеми инпутами формы, текст ошибки из resx
    """
    file_name, scenario_name = request.param
    scenarios = load_json_file(file_name)

    # Поиск нужного сценария (блока с тестовыми данными)
    data = next((s for s in scenarios if s["scenario_name"] == scenario_name), None)

    if data is None:
        all_names = [s.get("scenario_name") for s in scenarios]
        raise KeyError(
            f"\n[Критическая ошибка автотеста]:\n"
            f"Тест запросил сценарий с именем: '{scenario_name}'\n"
            f"Но в файле '{file_name}' такого сценария нет.\n"
            f"Список сценариев, которые ЕСТЬ в файле: {list(all_names)[:10]}...\n"
        )

    form_inputs = {}

    for key, value in data["inputs"].items():
        if value == "VALID_LOGIN":
            form_inputs[key] = config.user_name
        elif value == "VALID_PASSWORD":
            form_inputs[key] = config.user_password
        elif value == "VALID_EMAIL":
            form_inputs[key] = config.user_email
        else:
            form_inputs[key] = value

    # Проверяем, существует ли вообще такой ключ ошибки в .resx

    # Ключ для .resx ASP.NET
    raw_error_key = data.get("resx_error_key")
    # Ключ для value текста из JSON 
    # (строка ошибки для поиска совпадений, если ошибка из БД и ее нет в .resx)
    raw_error_text = data.get("errors")
    
    if raw_error_key:
        resx_key = raw_error_key.strip()

        current_lang = config.lang.lower()
        fallback_lang = "ru" if current_lang == "en" else "en"

        if resx_key in load_resx_strings[current_lang]:
            expected_errors = load_resx_strings[current_lang][resx_key]

        elif resx_key in load_resx_strings[fallback_lang]:
            expected_errors = load_resx_strings[fallback_lang][resx_key]

        else:
            all_available_keys = set(load_resx_strings["ru"].keys()) | set(
                load_resx_strings["en"].keys()
            )
            raise KeyError(
                f"\n[Критическая ошибка локализации]:\n"
                f"В JSON указан ключ ошибки '{resx_key}'\n"
                f"Но в .resx файле для языка '{config.lang}' такого ключа нет!\n"
                f"Возможно, имелся в виду один из этих ключей: {list(all_available_keys)[:10]}...\n"
            )
    elif raw_error_text and raw_error_text.strip():
        expected_errors = raw_error_text.strip()
    else:
        expected_errors = ""

    return MappingProxyType(form_inputs), expected_errors


@pytest.fixture(scope="session")
def load_resx_strings():
    ru_lang = "ru"
    en_lang = "en"
    strings = {ru_lang: {}, en_lang: {}}

    files_to_parse={ru_lang: _RU_ERRORS_RESX_PATH, en_lang: _EN_ERRORS_RESX_PATH}

    for lang, file_path in files_to_parse.items():
        if not file_path.exists():
            raise FileNotFoundError(
                f"Критическая ошибка: Файл локализации [{lang}] не найден по пути: {file_path}."
            )

        try:
            tree = ET.parse(file_path)
            root = tree.getroot()

            for item in root.findall("data"):
                name_key = item.get("name").strip()
                value_node = item.find("value")

                if name_key and value_node is not None:
                    clean_value = (value_node.text or "").strip()
                    strings[lang][name_key] = clean_value
        except ET.ParseError:
            raise ValueError(f"Ошибка синтаксиса XML в файле ресурсов: {file_path}")

    return strings

# endregion


# region API

T = TypeVar("T", bound=AsyncBaseClient)
M = TypeVar("M", bound=AppBaseModel)


def pytest_addoption(parser: pytest.Parser)->None:
    # Добавление в python.ini нового параметра - api_url для указания URL с портом API
    parser.addini(
        name="api_url",
        help="API URL для выполнения тестов",
        type="string"
    )


@pytest.fixture(scope="session")
def event_loop():
    policy = asyncio.get_event_loop_policy()
    loop = policy.new_event_loop()
    yield loop
    loop.close()


@pytest.fixture(scope="session")
def _base_url_from_config(request: pytest.FixtureRequest)->str:
    return request.config.getini("api_url")


@pytest.fixture(scope="session")
def auth_cookies(_base_url_from_config:str, config) ->httpx.Cookies:
    with httpx.Client(base_url=_base_url_from_config) as client:
        payload = LoginRequest(
            email=config.user_email, 
            password=config.user_password
        )
        
        try:
            login_response = client.post(
                "api/account/login",
                json=payload.model_dump(by_alias=True),
            )
            login_response.raise_for_status()
            return client.cookies
        except httpx.HTTPStatusError as exc:
            pytest.fail(f"Ошибка сети при попытке авторизации: {exc}")


# Обертка для клиента
@pytest_asyncio.fixture(scope="function", loop_scope="function")
async def httpx_client():
    async with httpx.AsyncClient(
        event_hooks={
            "request":[log_request],
            "response":[log_response]
        }
    ) as client:
        yield client


@pytest.fixture(scope="function")
def api_client_factory(
    httpx_client: httpx.AsyncClient,
    auth_cookies:httpx.Cookies,
    _base_url_from_config: str
):
    class ClientFactory:
        def __call__(self, client_class:Type[T])-> T:
            return client_class(client=httpx_client, base_url=_base_url_from_config)

        def authorized(self, client_class: Type[T]) -> T:
            httpx_client.cookies.update(auth_cookies)
            return client_class(client=httpx_client, base_url=_base_url_from_config)

    return ClientFactory()


async def log_request(request: httpx.Request) -> None:
    """Автоматически вызывается перед отправкой запроса."""
    print(f"\n🚀 [API REQUEST] {request.method} -> {request.url}")
    if request.content:
        try:
            # Отформатированное тело запроса
            body = json.loads(request.content.decode("utf-8"))
            print(f"📦 [BODY]: {json.dumps(body, indent=2, ensure_ascii=False)}")
        except Exception:
            print(f"📦 [BODY (raw)]: {request.content.decode('utf-8')}")


async def log_response(response: httpx.Response) -> None:
    await response.aread()
    """Автоматически вызывается при получении ответа."""
    print(f"✅ [API RESPONSE] Status: {response.status_code}")
    if response.text:
        try:
            # Отформатированное тело ответа
            body = json.loads(response.text)
            print(f"📥 [RESPONSE JSON]: {json.dumps(body, indent=2, ensure_ascii=False)}")
        except Exception:
            print(f"📥 [RESPONSE (raw)]: {response.text}")


# Парсинг данных в модель, чтобы обращаться через поля класса
@allure.step("Парсинг данных в модель: {model_class}")
def parse_to(self: httpx.Response, model_class:typing.Any) -> typing.Any:
        if not self.text or self.text.strip()=="":
            if model_class is bool:
                empty_model = self.is_success
            elif model_class in (int, str):
                empty_model = model_class()
            else:
                empty_model = TypeAdapter(model_class).validate_python({})
            allure.attach(
                empty_model.model_dump_json(indent=4) if hasattr(empty_model, 'model_dump_json') else str(empty_model),
                name="Результат парсинга (Пустой DTO)",
                attachment_type=allure.attachment_type.JSON
            )
            return empty_model
        
        if self.text.strip().startswith("["):
            is_expected_list = (
                typing.get_origin(model_class) is list or
                (isinstance(model_class, type) and issubclass(model_class, RootModel))
            )

            if is_expected_list:
                parsed_data = TypeAdapter(model_class).validate_json(self.text)

                log_content = parsed_data.model_dump_json(indent=4, by_alias=True) if hasattr(parsed_data, 'model_dump_json') else json.dumps(self.text, indent=4, ensure_ascii=False)
                allure.attach(
                    log_content,
                    name="Результат парсинга (Список ошибок)",
                    attachment_type=allure.attachment_type.JSON
                )
                return parsed_data
            else:
                parsed_list = TypeAdapter(list[str]).validate_json(self.text)
                allure.attach(
                    json.dumps(parsed_list,indent=4, ensure_ascii=False),
                    name="Результат парсинга (Список ошибок)",
                    attachment_type=allure.attachment_type.JSON
                )
                return parsed_list
        
        parsed_model = TypeAdapter(model_class).validate_json(self.text)
        allure.attach(
            parsed_model.model_dump_json(indent=4, by_alias=True),
            name="Результат парсинга (Валидный DTO)",
            attachment_type=allure.attachment_type.JSON
        )
        return parsed_model

# Регистрация внутри оригинального класса
httpx.Response.parse_to = parse_to

# endregion