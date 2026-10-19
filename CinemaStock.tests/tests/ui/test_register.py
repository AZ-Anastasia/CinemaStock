import allure
from playwright.sync_api._generated import Page
import pytest
from assertions.ui_verify import UIVerify
from conftest import TestConfig
from ui.pages.register_page import RegisterPage


@allure.epic("Регистрация")
@allure.feature("Форма регистрации (Register)")
@pytest.mark.ui
class TestRegister:
    _test_data_file_name = "register_data.json"

    @allure.id("001")
    @allure.tag("UI")
    @allure.story("Успешная регистрация")
    @allure.title("Попытка регистрации с валидными данными")
    @pytest.mark.smoke
    @pytest.mark.regress
    def test_register_success(self, page: Page, config: TestConfig):
        register_page = RegisterPage(page)
        register_page.open()
        user = config.generate_random_user()
        register_page.register(
            user["username"], user["email"], user["password"], user["password"]
        )

        UIVerify(register_page).url_should_contains("login")

    @allure.id("002")
    @allure.tag("UI")
    @allure.story("Неуспешная регистрация")
    @allure.title(
        "Попытка регистрации с невалидными данными: существующий логин, Email и пароль"
    )
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data",
        [(_test_data_file_name, "login_email_and_password_exist")],
        indirect=True,
    )
    def test_register_user_exist(self, page: Page, test_data: str):
        inputs, expected_errors = test_data

        register_page = RegisterPage(page)
        register_page.open()
        register_page.register(**inputs)

        UIVerify(register_page.get_error_text()).text_contains(expected_errors)

    @allure.id("003")
    @allure.tag("UI")
    @allure.story("Неуспешная регистрация")
    @allure.title("Попытка регистрации с невалидными данными: слабый пароль")
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data", [(_test_data_file_name, "weak_password")], indirect=True
    )
    def test_register_weak_password(self, page: Page, test_data: str):
        inputs, _ = test_data

        register_page = RegisterPage(page)
        register_page.open()

        register_page.register(**inputs)
        assert register_page.is_submit_button_enabled() is False

    @allure.id("004")
    @allure.tag("UI")
    @allure.story("Неуспешная регистрация")
    @allure.title("Попытка регистрации с невалидными данными: некорретный формат Email")
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data", [(_test_data_file_name, "invalid_email_format")], indirect=True
    )
    def test_register_invalid_email_format(self, page: Page, test_data: str):
        inputs, _ = test_data

        register_page = RegisterPage(page)
        register_page.open()

        register_page.register(**inputs)
        assert register_page.is_email_valid() is False

    @allure.id("005")
    @allure.tag("UI")
    @allure.story("Неуспешная регистрация")
    @allure.title("Попытка регистрации с невалидными данными: пароль слишком короткий")
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data",
        [
            (_test_data_file_name, "password_too_short"),
            (_test_data_file_name, "password_no_digits"),
            (_test_data_file_name, "password_doesnt_match")
        ],
        indirect=["test_data"],
    )
    def test_register_invalid_password(self, page: Page, test_data: str):
        inputs, scenario_name_value = test_data
        
        allure.dynamic.title(f"Попытка регистрации с невалидными данными. Валидация: {scenario_name_value}")

        register_page = RegisterPage(page)
        register_page.open()

        register_page.register(**inputs)
        assert register_page.is_submit_button_enabled() is False