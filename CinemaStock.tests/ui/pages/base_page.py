import allure
from playwright.sync_api import Locator
from conftest import ScreenshotService


class BasePage:
    URL_PATH="/"

    def __init__(self, page):
        self.page = page
        self.screenshot = ScreenshotService(page)
    
    @allure.step("Открыть страницу")
    def open(self):
        self.page.goto(self.URL_PATH)

        self.page.wait_for_load_state(state="load", timeout=5000)

        self.screenshot.capture(name=f"Страница {self.__class__.__name__} открыта")
        return self
    
    def get_error_text(self) -> str:
        return self._error_text.inner_text()
    
    def _is_field_valid(self, locator: Locator) -> bool:
        return locator.evaluate("node=> node.checkValidity()")