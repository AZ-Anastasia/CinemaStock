import allure
import httpx
from api.api_clients.base_client import AsyncBaseClient
from api.constants import HTTPMethod
from api.models.responses.id_name_response import IdNameResponse


class GenreClient(AsyncBaseClient):
    ROUTE_PREFIX = "/api/genres"

    def __init__(self, client: httpx.AsyncClient, base_url: str):
        super().__init__(client, base_url, route_prefix=self.ROUTE_PREFIX)

    @allure.step("API: Получение списка жанров")
    async def get_genres_list(self) -> httpx.Response:
        response = await self._send_request(
            method=HTTPMethod.GET,
            url="/list",
            follow_redirects=True,
            response_model = list[IdNameResponse]
        )
        return response

    @allure.step("API: Добавление жанра")
    async def add_genre(self, payload) -> httpx.Response:
        response = await self._send_request(
            method=HTTPMethod.POST,
            url="/",
            follow_redirects=True,
            json=payload.model_dump(by_alias=True, mode="json"),
            response_model = IdNameResponse
        )
        return response

    @allure.step("API: Редактирование жанра")
    async def update_genre(self, id, payload) -> httpx.Response:
        response = await self._send_request(
            method=HTTPMethod.PUT,
            url=f"/{id}",
            follow_redirects=True,
            json=payload.model_dump(by_alias=True, mode="json"),
            response_model = IdNameResponse
        )
        return response

    @allure.step("API: Удаление жанра")
    async def delete_genre(self, id) -> httpx.Response:
        response = await self._send_request(
            method=HTTPMethod.DELETE,
            url=f"/{id}",
            follow_redirects=True,
            response_model = bool
        )
        return response