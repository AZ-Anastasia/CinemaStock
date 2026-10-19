import allure
import pytest
from assertions.ui_verify import UIVerify
from ui.pages.login_page import LoginPage


@allure.epic("Авторизация и Аутентификация")
@allure.feature("Форма входа (Login)")
@pytest.mark.ui
class TestLogin:
    _test_data_file_name = "login_data.json"

    @allure.id("006")
    @allure.tag("UI")
    @allure.story("Успешный вход")
    @allure.title("Попытка входа с валидными данными")
    @pytest.mark.smoke
    @pytest.mark.regress
    def test_login_success(self, page, config):
        login_page = LoginPage(page)
        login_page.open()
        login_page.login(config.user_email, config.user_password)

        UIVerify(login_page).url_should_be("profile")

    @allure.id("007")
    @allure.tag("UI")
    @allure.story("Неуспешный вход")
    @allure.title("Попытка входа с невалидными данными: неверный пароль")
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data", [(_test_data_file_name, "invalid_password")], indirect=True
    )
    def test_login_invalid_password(self, page, test_data):
        inputs, expected_errors = test_data

        login_page = LoginPage(page)
        login_page.open()
        login_page.login(**inputs)

        UIVerify(login_page.get_error_text()).text_contains(expected_errors)

    @allure.id("008")
    @allure.tag("UI")
    @allure.story("Неуспешный вход")
    @allure.title("Попытка входа с невалидными данными: неверный Email")
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data", [(_test_data_file_name, "invalid_name")], indirect=True
    )
    def test_login_invalid_email(self, page, test_data):
        inputs, expected_errors = test_data

        login_page = LoginPage(page)
        login_page.open()
        login_page.login(**inputs)

        UIVerify(login_page.get_error_text()).text_contains(expected_errors)

    @allure.id("009")
    @allure.tag("UI")
    @allure.story("Неуспешный вход")
    @allure.title("Попытка входа с невалидными данными: пустой Email")
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data", [(_test_data_file_name, "empty_email_field")], indirect=True
    )
    def test_login_empty_email(self, page, test_data):
        inputs, _ = test_data

        login_page = LoginPage(page)
        login_page.open()
        login_page.login(**inputs)

        assert login_page.is_email_valid() is False

    @allure.id("010")
    @allure.tag("UI")
    @allure.story("Неуспешный вход")
    @allure.title("Попытка входа с невалидными данными: пустой пароль")
    @pytest.mark.regress
    @pytest.mark.parametrize(
        "test_data", [(_test_data_file_name, "empty_password_field")], indirect=True
    )
    def test_login_empty_password(self, page, test_data):
        inputs, _ = test_data

        login_page = LoginPage(page)
        login_page.open()
        login_page.login(**inputs)

        assert login_page.is_password_valid() is False