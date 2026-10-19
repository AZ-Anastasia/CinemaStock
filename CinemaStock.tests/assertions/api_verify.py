from typing import Any
import allure


class APIVerify:
    def __init__(self, dto: Any):
        self.dto = dto

    @allure.step("Проверка API: Список ошибок содержит текст '{expected_error}'")
    def errors_contain(self, expected_error: str) -> "APIVerify":
        if isinstance(self.dto, list) and all(isinstance(x, str) for x in self.dto):
            if not expected_error or expected_error.strip() == "":
                assert (
                    len(self.dto) > 0
                ), "Ошибка! Сервер вернул успешный статус или пустой список ошибок для пустого поля"
                return self

            has_error = any(expected_error in str(err) for err in self.dto)
            assert has_error, (
                f"Ошибка! Не найден текст '{expected_error}' в ответе API.\n"
                f"Фактический ответ сервера: {self.dto}"
            )
            return self

        if isinstance(self.dto, list):
            all_errors = []
            for item in self.dto:
                err_field = getattr(item, "errors", getattr(item, "Errors", None))
                if isinstance(err_field, list):
                    all_errors.extend(err_field)
                elif err_field:
                    all_errors.append(err_field)

            if not expected_error or expected_error.strip() == "":
                assert (
                    len(all_errors) > 0
                ), "Ошибка! Сервер вернул пустой список ошибок для пустого поля"
                return self

            has_error = any(expected_error in str(err) for err in all_errors)
            assert has_error, (
                f"Ошибка! Не найден текст '{expected_error}' в ответе API.\n"
                f"Список ошибок: {all_errors}"
            )
            return self

        errors = getattr(self.dto, "errors", getattr(self.dto, "Errors", None))

        if not expected_error or expected_error.strip() == "":
            assert errors is not None, f"Ошибка! Объект {type(self.dto).__name__} не содержит поле errors."
            assert (
                len(errors) > 0
            ), "Ошибка! Сервер вернул успешный статус или пустой список ошибок для пустого поля"
            return self

        assert errors is not None, (
            f"Ошибка! Сервер в объекте {type(self.dto).__name__} вернул null/None вместо списка ошибок.\n"
            f"Ожидали увидеть: '{expected_error}'"
        )

        if isinstance(errors, list):
            has_error = any(expected_error in str(err) for err in errors)
        else:
            has_error = expected_error in str(errors)

        assert has_error, (
            f"Ошибка! Не найден текст '{expected_error}' в ответе API.\n"
            f"Список ошибок: {errors}"
        )

        return self