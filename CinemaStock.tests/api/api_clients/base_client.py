import json
import allure
import httpx
from typing import TypeVar, Type
from pydantic import BaseModel

T = TypeVar('T', bound=BaseModel)

class AsyncBaseClient:
    def __init__(self, client: httpx.AsyncClient, base_url: str, route_prefix: str = ""):
        self.client = client
        self.base_url = f"{base_url}{route_prefix}"
    
    # **kwargs - любые параметры
    # T | dict - на выходе получаем или готовую Pydantic-модель (T), или обычный словарь dict
    async def _send_request(self, method: str, url: str, response_model: Type[T] = None, **kwargs) -> T | dict:
        full_url = f"{self.base_url}{url}"
        kwargs.setdefault("timeout", 15.0)

        # Выносим метод и URL в название шага Allure
        step_name = f"API Request: {method.upper()} {url}"
        
        with allure.step(step_name):
            # Логируем параметры запроса, если они переданы
            self._attach_request_data(kwargs)

            try:
                # Отправляем сам запрос
                response = await self.client.request(method, full_url, **kwargs)
                
                # Логируем ответ сервера (Статус и Тело)
                self._attach_response_data(response)

                response.parsed = None

                if response.text.strip():
                    try:
                        response_json = response.json()
                        if response_model:
                            response.parsed = response_model.model_validate(response_json)
                        else:
                            response.parsed = response_json
                    except Exception as parse_error:
                        allure.attach(
                            f"Не удалось распарсить JSON. Ошибка: {str(parse_error)}\nТело: {response.text[:500]}",
                            name="Парсинг лог",
                            attachment_type=allure.attachment_type.TEXT
                        )
                        raise ValueError(f"Сервер вернул не JSON. Текст ответа содержит: {str(parse_error)} {response.text[:200]}")
                return response
            
            except Exception as e:
                # Если запрос упал с ошибкой, Allure автоматически подсветит этот шаг красным
                allure.attach(str(e), name="Request Exception", attachment_type=allure.attachment_type.TEXT)
                raise e


    def _attach_request_data(self, kwargs: dict):
        """Вспомогательный метод для логирования отправляемых данных."""
        # Логируем JSON payload
        if "json" in kwargs and kwargs["json"]:
            allure.attach(
                json.dumps(kwargs["json"], indent=4, ensure_ascii=False),
                name="Request Body (JSON)",
                attachment_type=allure.attachment_type.JSON
            )
        # Логируем Query параметры (?param=value)
        if "params" in kwargs and kwargs["params"]:
            allure.attach(
                json.dumps(kwargs["params"], indent=4, ensure_ascii=False),
                name="Request Query Params",
                attachment_type=allure.attachment_type.JSON
            )
        # Логируем заголовки, если нужно (скрывая чувствительные токены)
        if "headers" in kwargs and kwargs["headers"]:
            clean_headers = {k: (v if k.lower() != "authorization" else "Bearer ***") for k, v in kwargs["headers"].items()}
            allure.attach(
                json.dumps(clean_headers, indent=4),
                name="Request Headers",
                attachment_type=allure.attachment_type.JSON
            )

    def _attach_response_data(self, response: httpx.Response):
        """Вспомогательный метод для логирования полученного ответа."""
        # Добавляем текстовый аттач со статус-кодом
        allure.attach(
            f"Status Code: {response.status_code} {response.reason_phrase}", 
            name="Response Status", 
            attachment_type=allure.attachment_type.TEXT
        )

        if not response.text.strip():
            allure.attach("Empty Response Body", name="Response Body (EMPTY)", attachment_type=allure.attachment_type.TEXT)
            return
        
        # Пробуем прикрепить тело как красивый JSON, иначе как обычный текст
        try:
            parsed_json = json.loads(response.text)
            allure.attach(
                json.dumps(parsed_json, indent=4, ensure_ascii=False),
                name="Response Body (JSON)",
                attachment_type=allure.attachment_type.JSON
            )
        except (ValueError, json.JSONDecodeError):
            if response.text:
                allure.attach(
                    response.text,
                    name="Response Body (RAW)",
                    attachment_type=allure.attachment_type.TEXT
                )