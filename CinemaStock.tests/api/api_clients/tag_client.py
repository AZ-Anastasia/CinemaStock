import allure
import httpx
from api.api_clients.base_client import AsyncBaseClient
from api.constants import HTTPMethod
from api.models.responses.id_name_response import IdNameResponse


class TagClient(AsyncBaseClient):
    ROUTE_PREFIX = "/api/tags"

    def __init__(self, client: httpx.AsyncClient, base_url: str):
        super().__init__(client, base_url, route_prefix=self.ROUTE_PREFIX)

    @allure.step("API: Получение списка тегов")
    async def get_tags_list(self) -> httpx.Response:
        response = await self._send_request(
            method=HTTPMethod.GET,
            url="/list",
            follow_redirects=True,
            response_model=list[IdNameResponse],
        )
        return response

    @allure.step("API: Добавление тега")
    async def add_tag(self, payload) -> httpx.Response:
        response = await self._send_request(
            method=HTTPMethod.POST,
            url="/",
            follow_redirects=True,
            json=payload.model_dump(by_alias=True, mode="json"),
            response_model=IdNameResponse,
        )
        return response

    @allure.step("API: Редактирование тега")
    async def update_tag(self, id, payload) -> httpx.Response:
        response = await self._send_request(
            method=HTTPMethod.PUT,
            url=f"/{id}",
            follow_redirects=True,
            json=payload.model_dump(by_alias=True, mode="json"),
            response_model=IdNameResponse,
        )
        return response

    @allure.step("API: Удаление тега")
    async def delete_tag(self, id) -> httpx.Response:
        response = await self._send_request(
            method=HTTPMethod.DELETE,
            url=f"/{id}",
            follow_redirects=True,
            response_model=bool,
        )
        return response
