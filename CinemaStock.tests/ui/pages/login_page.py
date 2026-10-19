import allure
from ui.pages.base_page import BasePage
from playwright.sync_api import Page


class LoginPage(BasePage):
    URL_PATH = "/login"

    def __init__(self, page: Page):
        super().__init__(page)

        self._email_input = page.locator("#email")
        self._password_input = page.locator("#password")
        self._submit_button = page.locator("button[type='submit']")
        self._error_text = page.locator(".error-text")
    
    @allure.step("Попытка входа в систему под пользователем: {email}")
    def login(self, email, password):
        self._email_input.fill(email)
        self._password_input.fill(password)
        self._submit_button.click()

        self.screenshot.capture(f"Форма входа заполнена. Пользователь: {email}")
    
    @allure.step("Проверка: Является ли Email валидным")
    def is_email_valid(self) -> bool:
        result = self._is_field_valid(self._email_input)
        status_text = "Валидный" if result else "Невалидный"

        with allure.step(f"> {status_text}"):
            pass
        return result
    
    @allure.step("Проверка: Является ли пароль валидным")
    def is_password_valid(self) -> bool:
        result = self._is_field_valid(self._password_input)
        status_text = "Валидный" if result else "Невалидный"

        with allure.step(f"> {status_text}"):
            pass
        
        return result