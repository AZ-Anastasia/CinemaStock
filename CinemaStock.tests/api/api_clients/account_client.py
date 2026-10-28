import allure
import httpx
from api.api_clients.base_client import AsyncBaseClient
from api.constants import HTTPMethod
from api.models.responses.user_info_response import UserInfoResponse
from api.models.result_dto import ResultDto


class AccountClient(AsyncBaseClient):
    ROUTE_PREFIX = "/api/account"

    def __init__(self, client: httpx.AsyncClient, base_url:str):
        super().__init__(client, base_url, route_prefix=self.ROUTE_PREFIX)
    
    @allure.step("API: Получение текущего пользователя")
    async def get_current_user(self)-> httpx.Response:
        return await self._send_request(
            method=HTTPMethod.GET,
            url="/current",
            follow_redirects=True,
            response_model=ResultDto[UserInfoResponse]
        )
    
    @allure.step("API: Выполнение входа в систему")
    async def login(self, payload)-> httpx.Response:
        return await self._send_request(
            method=HTTPMethod.POST,
            url="/login",
            follow_redirects=True,
            json=payload.model_dump(by_alias=True),
            response_model=ResultDto
        )
    
    @allure.step("API: Выполнение регистрации в системе")
    async def register(self, payload: dict)-> httpx.Response:
        return await self._send_request(
            method=HTTPMethod.POST,
            url="/register",
            json=payload.model_dump(by_alias=True),
            response_model=ResultDto
        )
    
    @allure.step("API: Выполнение выхода из системы")
    async def logout(self)-> httpx.Response:
        return await self._send_request(
            method=HTTPMethod.POST,
            url="/logout",
            follow_redirects=True,
            response_model=bool
        )