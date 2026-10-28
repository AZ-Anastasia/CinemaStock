import allure
import pytest
from api.api_clients.account_client import AccountClient
from api.models.requests.login_request import LoginRequest
from api.models.requests.register_request import RegisterRequest
from api.models.responses.user_base_info_response import UserBaseInfoResponse
from api.models.responses.user_info_response import UserInfoResponse
from api.models.result_dto import ResultDto
from assertions.api_verify import APIVerify


@allure.epic("Информация о пользователе")
@allure.feature("API информация о пользователе")
@pytest.mark.api
class TestAccountInfoAPI:
    @allure.id("022")
    @allure.tag("API")
    @allure.story("Получение информации о пользователе")
    @allure.title("Получение текущего пользователя")
    @pytest.mark.smoke
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_get_current_user(self, config, api_client_factory):
        client = api_client_factory(AccountClient)

        payload = LoginRequest(
            email=config.user_email, 
            password=config.user_password
        )

        response = await client.login(payload)
        assert response.status_code==200
        
        dto = response.parse_to(ResultDto[UserInfoResponse])
        assert dto.is_success is True
        assert (
            dto.errors is None
        ), "При успешном запросе список ошибок должен быть пуст"
        assert dto.data.email == payload.email
        assert "Admin" in dto.data.roles

        response_user = await client.get_current_user()
        assert response_user.status_code==200

        user = response_user.parse_to(ResultDto[UserInfoResponse])
        assert user.data.email == payload.email

        response_logout = await client.logout()
        assert response_logout.status_code==200
        
        result = response_logout.parse_to(bool)
        assert result is True


@allure.epic("Регистрация")
@allure.feature("API регистрации")
@pytest.mark.api
class TestAccountRegisterAPI:
    _test_data_file_name = "register_data.json"

    @allure.id("011")
    @allure.tag("API")
    @allure.story("Успешная регистрация")
    @allure.title("Попытка регистрации с валидными данными")
    @pytest.mark.smoke
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_register_success(self, config, api_client_factory):
        client = api_client_factory(AccountClient)
        user = config.generate_random_user()

        payload = RegisterRequest(
            username=user["username"], 
            email=user["email"], 
            password=user["password"]
        )

        response = await client.register(payload)
        assert response.status_code==200

        dto = response.parse_to(ResultDto[UserBaseInfoResponse])
        assert dto.is_success is True
        assert (
            dto.errors is None
        ), "При успешном запросе список ошибок должен быть пуст"
        assert dto.data.username == payload.username
        assert dto.data.email == payload.email

    @allure.id("012")
    @allure.tag("API")
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
    @pytest.mark.asyncio
    async def test_register_user_exist(self, test_data, api_client_factory):
        client = api_client_factory(AccountClient)
        user_data, expected_errors = test_data

        payload = RegisterRequest(
            username=user_data["username"], 
            email=user_data["email"], 
            password=user_data["password"]
        )

        response = await client.register(payload)
        assert response.status_code==400

        dto = response.parse_to(ResultDto[UserBaseInfoResponse])
        assert dto.is_success is False
        APIVerify(dto).errors_contain(expected_errors)


    @allure.id("013")
    @allure.tag("API")
    @allure.story("Неуспешная регистрация")
    @allure.title("Попытка регистрации с невалидными данными: слабый пароль")
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data", [(_test_data_file_name, "weak_password_api")], indirect=True
    )
    @pytest.mark.asyncio
    async def test_register_weak_password(self, test_data, api_client_factory):
        client = api_client_factory(AccountClient)
        user_data, expected_errors = test_data

        payload = RegisterRequest(
            username=user_data["username"], 
            email=user_data["email"], 
            password=user_data["password"]
        )

        response = await client.register(payload)
        assert response.status_code==400
        
        dto = response.parse_to(ResultDto[UserBaseInfoResponse])
        APIVerify(dto).errors_contain(expected_errors)


    @allure.id("014")
    @allure.tag("API")
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
    @pytest.mark.asyncio
    async def test_register_invalid_password(self, test_data, api_client_factory):
        client = api_client_factory(AccountClient)
        user_data, scenario_name_value = test_data
        allure.dynamic.title(f"Попытка регистрации с невалидными данными. Валидация: {scenario_name_value}")

        payload = RegisterRequest(
            username=user_data["username"], 
            email=user_data["email"], 
            password=user_data["password"]
        )

        response = await client.register(payload)
        assert response.status_code==400
        
        dto = response.parse_to(ResultDto[UserBaseInfoResponse])
        APIVerify(dto).errors_contain(scenario_name_value)


@allure.epic("Вход")
@allure.feature("API входа")
@pytest.mark.api
class TestAccountLoginAPI:
    _test_data_file_name = "login_data.json"
    
    @allure.id("015")
    @allure.tag("API")
    @allure.story("Успешный вход")
    @allure.title("Попытка входа с валидными данными")
    @pytest.mark.smoke
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_login_success(self, config, api_client_factory):
        client = api_client_factory(AccountClient)

        payload = LoginRequest(
            email=config.user_email, 
            password=config.user_password
        )

        response = await client.login(payload)
        assert response.status_code==200
        
        dto = response.parse_to(ResultDto[UserInfoResponse])
        assert dto.is_success is True
        assert (
            dto.errors is None
        ), "При успешном запросе список ошибок должен быть пуст"
        assert dto.data.email == payload.email
        assert "Admin" in dto.data.roles

    @allure.id("016")
    @allure.tag("API")
    @allure.story("Неуспешный вход")
    @allure.title("Попытка входа с невалидными данными: неверный пароль")
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data", [(_test_data_file_name, "invalid_password")], indirect=True
    )
    @pytest.mark.asyncio
    async def test_login_invalid_password(self, test_data, api_client_factory):
        client = api_client_factory(AccountClient)
        user_data, expected_errors = test_data

        payload = LoginRequest(
            email=user_data["email"], 
            password=user_data["password"]
        )

        response = await client.login(payload)
        assert response.status_code==400
        
        dto = response.parse_to(ResultDto[UserBaseInfoResponse])
        APIVerify(dto).errors_contain(expected_errors)


    @allure.id("017")
    @allure.tag("API")
    @allure.story("Неуспешный вход")
    @allure.title("Попытка входа с невалидными данными: неверный Email")
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data", [(_test_data_file_name, "invalid_name")], indirect=True
    )
    @pytest.mark.asyncio
    async def test_login_invalid_email(self, test_data, api_client_factory):
        client = api_client_factory(AccountClient)
        user_data, expected_errors = test_data

        payload = LoginRequest(
            email=user_data["email"], 
            password=user_data["password"]
        )

        response = await client.login(payload)
        assert response.status_code==400
        
        dto = response.parse_to(ResultDto[UserBaseInfoResponse])
        APIVerify(dto).errors_contain(expected_errors)


    @allure.id("018")
    @allure.tag("API")
    @allure.story("Неуспешный вход")
    @allure.title("Попытка входа с невалидными данными: пустой Email")
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data", [(_test_data_file_name, "empty_email_field")], indirect=True
    )
    @pytest.mark.asyncio
    async def test_login_empty_email(self, test_data, api_client_factory):
        client = api_client_factory(AccountClient)
        user_data, expected_errors = test_data

        payload = LoginRequest(
            email=user_data["email"], 
            password=user_data["password"]
        )

        response = await client.login(payload)
        assert response.status_code==400
        
        dto = response.parse_to(ResultDto[UserBaseInfoResponse])
        APIVerify(dto).errors_contain(expected_errors)


    @allure.id("019")
    @allure.tag("API")
    @allure.story("Неуспешный вход")
    @allure.title("Попытка входа с невалидными данными: пустой пароль")
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data", [(_test_data_file_name, "empty_password_field")], indirect=True
    )
    @pytest.mark.asyncio
    async def test_login_empty_password(self, test_data, api_client_factory):
        client = api_client_factory(AccountClient)
        user_data, expected_errors = test_data

        payload = LoginRequest(
            email=user_data["email"], 
            password=user_data["password"]
        )

        response = await client.login(payload)
        assert response.status_code==400
        
        dto = response.parse_to(ResultDto[UserBaseInfoResponse])
        APIVerify(dto).errors_contain(expected_errors)


    @allure.id("020")
    @allure.tag("API")
    @allure.story("Успешный выход")
    @allure.title("Попытка выхода с авторизированным пользователем")
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_authorized_logout(self, config, api_client_factory):
        client = api_client_factory(AccountClient)

        payload = LoginRequest(
            email=config.user_email, 
            password=config.user_password
        )

        response = await client.login(payload)
        assert response.status_code==200
        
        dto = response.parse_to(ResultDto[UserInfoResponse])
        assert dto.is_success is True
        assert (
            dto.errors is None
        ), "При успешном запросе список ошибок должен быть пуст"
        assert dto.data.email == payload.email
        assert "Admin" in dto.data.roles

        response_logout = await client.logout()
        assert response_logout.status_code==200
        
        result = response_logout.parse_to(bool)
        assert result is True


    @allure.id("021")
    @allure.tag("API")
    @allure.story("Неуспешный выход")
    @allure.title("Попытка выхода с неавторизированным пользователем")
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_unauthorized_logout(self, api_client_factory):
        client = api_client_factory(AccountClient)

        response_logout = await client.logout()
        assert response_logout.status_code==401