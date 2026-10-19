import allure
from ui.pages.base_page import BasePage
from playwright.sync_api import Page


class RegisterPage(BasePage):
    URL_PATH="/register"

    def __init__(self, page: Page):
        super().__init__(page)
        
        self._username_input = page.locator("#reg-username")
        self._email_input = page.locator("#email")
        self._password_input = page.locator("#reg-password")
        self._confirm_password_input = page.locator("#confirm-password")
        self._submit_button = page.locator("button[type='submit']")
        self._error_text = page.locator(".error-text")
        self._password_tooltip = page.locator(".tooltip-window")
    
    @allure.step("Заполнение формы регистрации для пользователя: {email}")
    def fill_registration_form(self, username, email, password, confirm_password):
        self._username_input.fill(username)
        self._email_input.fill(email)

        self._password_input.fill(password)
        self._confirm_password_input.fill(confirm_password)

        self.screenshot.capture(f"Форма регистрации заполнена. Пользователь: {email}")
    
    @allure.step("Выполнение регистрации в системе под пользователем: {email}")
    def register(self, username, email, password, confirm_password):
        self.fill_registration_form(username, email, password, confirm_password)

        if self.is_submit_button_enabled(log_step=False):
            self._submit_button.click()

        self.screenshot.capture(f"Форма регистрации заполнена. Пользователь: {email}")
    
    def is_submit_button_enabled(self, log_step=False) -> bool:
        def _check():
            result = self._submit_button.is_enabled(timeout=0)
            status_text = "Кнопка доступна для нажатия" if result else "Кнопка недоступна для нажатия"

            with allure.step(f"> {status_text}"):
                pass
            return result
        
        if log_step:
            return allure.step("Проверка: Доступность кнопки регистрации")
        
        return _check()

    @allure.step("Проверка: Является ли Email валидным")
    def is_email_valid(self) -> bool:
        result = self._is_field_valid(self._email_input)
        status_text = "Валидный" if result else "Невалидный"

        with allure.step(f"> {status_text}"):
            pass
        return result
    
    @property
    def password_tooltip(self):
        return self.password_tooltip