from typing import List
import uuid
import allure
import pytest

from api.api_clients.tag_client import TagClient
from api.models.responses.id_name_response import IdNameResponse


@allure.epic("Теги")
@allure.feature("API тегов")
@pytest.mark.api
class TestTagsAPI:
    @allure.id("033")
    @allure.tag("API")
    @allure.story("Получение списка тегов")
    @allure.title("Получение списка тегов")
    @pytest.mark.smoke
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_get_tags_list(self, api_client_factory):
        client = api_client_factory(TagClient)

        response = await client.get_tags_list()
        assert response.status_code == 200

        tags = response.parse_to(List[IdNameResponse])
        assert isinstance(tags, list), "Ответ не является списком list"
        assert len(tags) > 0, "В ответе получен пустой список"

    @allure.id("034")
    @allure.tag("API")
    @allure.story("Добавление тега")
    @allure.title("Успешное добавление нового тега с валидными данными")
    @pytest.mark.smoke
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_add_tag(self, api_client_factory):
        client = api_client_factory.authorized(TagClient)

        tag_name = "Фэнтези"
        tag_to_add = IdNameResponse(Id=uuid.uuid4(), Name=tag_name)
        tag = None

        try:
            with allure.step("Добавление тега"):
                response_added_tag = await client.add_tag(tag_to_add)
                assert response_added_tag.status_code == 200

                tag = response_added_tag.parse_to(IdNameResponse)
                assert tag.name == tag_to_add.name

        finally:
            if tag:
                with allure.step("Удаление добавленного тега"):
                    response_deleted_tag = await client.delete_tag(tag.id)
                    assert response_deleted_tag.status_code == 200

                    deleted_tag = response_deleted_tag.parse_to(bool)
                    assert deleted_tag == True

    @allure.id("035")
    @allure.tag("API")
    @allure.story("Добавление тега")
    @allure.title("Попытка создания дубликата тега")
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_add_tag_duplicate(self, api_client_factory):
        client = api_client_factory.authorized(TagClient)

        tag_name = "TestTag"
        tag_to_add = IdNameResponse(Id=uuid.uuid4(), Name=tag_name)
        tag = None

        try:
            with allure.step("Добавление тега"):
                response_added_tag = await client.add_tag(tag_to_add)
                assert response_added_tag.status_code == 200

                tag = response_added_tag.parse_to(IdNameResponse)
                assert tag.name == tag_to_add.name

                with allure.step("Добавление дубликата тега"):
                    response_added_tag = await client.add_tag(tag_to_add)
                    assert response_added_tag.status_code == 400

        finally:
            if tag:
                with allure.step("Удаление добавленного тега"):
                    response_deleted_tag = await client.delete_tag(tag.id)
                    assert response_deleted_tag.status_code == 200

                    deleted_tag = response_deleted_tag.parse_to(bool)
                    assert deleted_tag == True

    @allure.id("036")
    @allure.tag("API")
    @allure.story("Разграничение ролей")
    @allure.title(
        "Анонимный пользователь не имеет доступа к функционалу добавлению тега"
    )
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_add_tag_unauthorized_user(self, api_client_factory):
        client = api_client_factory(TagClient)

        tag_name = "TestTag"
        tag_to_add = IdNameResponse(Id=uuid.uuid4(), Name=tag_name)

        response_added_tag = await client.add_tag(tag_to_add)
        assert response_added_tag.status_code == 401

    @allure.id("037")
    @allure.tag("API")
    @allure.story("Добавление тега")
    @allure.title("Попытка создания тега с невалидным именем")
    @pytest.mark.regress
    @pytest.mark.asyncio
    @pytest.mark.parametrize("invalid_name", ["", "   ", "A" * 256])
    async def test_add_tag_invalid_name(self, api_client_factory, invalid_name):
        client = api_client_factory.authorized(TagClient)

        tag_to_add = IdNameResponse(Id=uuid.uuid4(), Name=invalid_name)

        with allure.step(f"Добавление тега с именем: '{invalid_name}'"):
            response = await client.add_tag(tag_to_add)
            assert response.status_code == 400

    @allure.id("038")
    @allure.tag("API")
    @allure.story("Редактирование тега")
    @allure.title("Успешное редактирование нового тега с валидными данными")
    @pytest.mark.smoke
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_update_tag(self, api_client_factory):
        client = api_client_factory.authorized(TagClient)
        tag_updated = None

        with allure.step("Получение тега из списка тегов"):
            response_list = await client.get_tags_list()
            assert response_list.status_code == 200

            tags = response_list.parse_to(List[IdNameResponse])
            assert isinstance(tags, list), "Ответ не является списком list"
            assert len(tags) > 0, "В ответе получен пустой список"

            tag_to_update = tags[0]
            original_name = tag_to_update.name
            tag_to_update.name = "TestTag"

        try:
            with allure.step("Обновление названия тега"):
                response_update = await client.update_tag(
                    tag_to_update.id, tag_to_update
                )
                assert response_update.status_code == 200

                tag_updated = response_update.parse_to(IdNameResponse)
                assert tag_updated.name == tag_to_update.name

        finally:
            if tag_updated:
                with allure.step("Восстановление названия тега"):
                    tag_to_update.name = original_name
                    response_update = await client.update_tag(
                        tag_to_update.id, tag_to_update
                    )
                    assert response_update.status_code == 200

                    response_list = await client.get_tags_list()
                    assert response_list.status_code == 200

                    tags = response_list.parse_to(List[IdNameResponse])

                    fresh_tag = next(g for g in tags if g.id == tag_to_update.id)
                    assert fresh_tag.name == original_name

    @allure.id("039")
    @allure.tag("API")
    @allure.story("Редактирование тега")
    @allure.title("Попытка изменения имени на уже существующее")
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_update_tag_already_exist(self, api_client_factory):
        client = api_client_factory.authorized(TagClient)

        with allure.step("Получение тега из списка тегов"):
            response_list = await client.get_tags_list()
            assert response_list.status_code == 200

            tags = response_list.parse_to(List[IdNameResponse])
            assert isinstance(tags, list), "Ответ не является списком list"
            assert len(tags) >= 2, "Для теста необходимо как минимум 2 тега"

            tag_to_update1 = tags[0]
            tag_to_update2 = tags[1]

        with allure.step("Попытка переименования первого тега в имя второго"):
            bad_payload = IdNameResponse(
                Id=tag_to_update1.id, Name=tag_to_update2.name
            )

            response_update = await client.update_tag(
                tag_to_update1.id, bad_payload
            )
            assert response_update.status_code == 400

        with allure.step("Проверка, что первый тег не изменился в БД"):
            response_list_after = await client.get_tags_list()
            tags_after = response_list_after.parse_to(List[IdNameResponse])

            fresh_tag = next(g for g in tags_after if g.id == tag_to_update1.id)
            assert fresh_tag.name == tag_to_update1.name

    @allure.id("040")
    @allure.tag("API")
    @allure.story("Редактирование тега")
    @allure.title("Попытка изменения несуществующего тега")
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_update_non_existent_tag(self, api_client_factory):
        client = api_client_factory.authorized(TagClient)

        non_existent_id = uuid.uuid4()
        payload = IdNameResponse(Id=non_existent_id, Name="UpdateNonExistent")

        with allure.step("Отправка запроса с несуществующим ID тега"):
            response = await client.update_tag(non_existent_id, payload)
            assert response.status_code == 400

    @allure.id("041")
    @allure.tag("API")
    @allure.story("Удаление тега")
    @allure.title("Успешное удаление тега с валидными данными")
    @pytest.mark.smoke
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_delete_tag(self, api_client_factory):
        client = api_client_factory.authorized(TagClient)
        deleted_tag = None

        with allure.step("Получение тега из списка тегов"):
            response_list = await client.get_tags_list()
            assert response_list.status_code == 200

            tags = response_list.parse_to(List[IdNameResponse])
            assert isinstance(tags, list), "Ответ не является списком list"
            assert len(tags) > 0, "В ответе получен пустой список"

            tag_to_delete = tags[0]

        try:
            with allure.step("Удаление тега"):
                response_deleted_tag = await client.delete_tag(tag_to_delete.id)
                assert response_deleted_tag.status_code == 200

                deleted_tag = response_deleted_tag.parse_to(bool)
                assert deleted_tag == True

        finally:
            if deleted_tag:
                with allure.step("Добавление тега"):
                    response_added_tag = await client.add_tag(tag_to_delete)
                    assert response_added_tag.status_code == 200

                    tag = response_added_tag.parse_to(IdNameResponse)
                    assert tag.name == tag_to_delete.name

    @allure.id("042")
    @allure.tag("API")
    @allure.story("Удаление тега")
    @allure.title("Попытка удаления несуществующего тега")
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_delete_non_existent_tag(self, api_client_factory):
        client = api_client_factory.authorized(TagClient)

        non_existent_id = uuid.uuid4()

        with allure.step("Отправка запроса с несуществующим ID тега"):
            response = await client.delete_tag(non_existent_id)
            assert response.status_code == 404
