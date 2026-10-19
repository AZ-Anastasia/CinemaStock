import re
import allure
from playwright.sync_api import expect, Page, Locator
from conftest import ScreenshotService


class UIVerify:
    def __init__(self, actual_value):
        self.actual = actual_value
        self.screenshot = ScreenshotService(self.page)

    @property
    def page(self) -> Page:
        obj = getattr(self.actual, "page", self.actual)

        # Если объект - локатор, то берем page
        if isinstance(obj, Locator):
            return obj.page

        return obj

    @allure.step("Проверка: Текст элемента содержит '{expected_text}'")
    def text_contains(self, expected_text):
        assert (
            expected_text in self.actual
        ), f"Ошибка! Ожидали '{expected_text}', но на экране отображается '{self.actual}'"

    @allure.step("Проверка: Ссылка равна '{expected_url}'")
    def url_should_be(self, expected_url, timeout=3000):
        # Извлекаем нативный объект Playwright, если была передана обертка
        # Если есть атрибут 'node' или 'page', то берем его. Либо объект
        native_node = getattr(
            self.actual, "node", getattr(self.actual, "page", self.actual)
        )

        self.screenshot.capture(name=f"Страница по ссылке: {expected_url}")
        expect(native_node).to_have_url(expected_url, timeout=timeout)

    @allure.step("Проверка: Ссылка содержит '{expected_url_substring}'")
    def url_should_contains(self, expected_url_substring: str, timeout: float = 3000):
        # Извлекаем нативный объект Playwright, если была передана обертка
        # Если есть атрибут 'node' или 'page', то берем его. Либо объект
        native_node = getattr(
            self.actual, "node", getattr(self.actual, "page", self.actual)
        )
        regex = re.compile(re.escape(expected_url_substring))

        self.screenshot.capture(name=f"Страница по ссылке: {expected_url_substring}")
        expect(native_node).to_have_url(regex, timeout=timeout)

    @allure.step("Проверка UI: Правило тултипа по индексу {index} имеет статус 'valid' == {expected_valid}")
    def password_rule_is_valid(self, index: int, expected_valid: bool, timeout: float = 3000) -> "UIVerify":
        """Проверяет наличие css-класса 'valid' у элемента списка внутри тултипа."""
        # Извлекаем нативный объект Playwright (работает через свойство page)
        native_locator = getattr(
            self.actual, "node", getattr(self.actual, "page", self.actual)
        )
        
        # Находим нужный li внутри переданного локатора тултипа
        target_li = native_locator.locator("ul > li").nth(index)
        
        # Делаем скриншот перед проверкой (использует ScreenshotService)
        self.screenshot.capture(name=f"Проверка правила тултипа под индексом {index}")
        
        # Нативный expect от Playwright сам подождет обновления классов в DOM
        if expected_valid:
            expect(target_li).to_have_class(re.compile(r"\bvalid\b"), timeout=timeout)
        else:
            expect(target_li).not_to_have_class(re.compile(r"\bvalid\b"), timeout=timeout)
            
        return self