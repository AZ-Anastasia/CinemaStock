from typing import List
import uuid
import allure
import pytest

from api.api_clients.genre_client import GenreClient
from api.models.responses.id_name_response import IdNameResponse


@allure.epic("Жанры")
@allure.feature("API жанров")
@pytest.mark.api
class TestGenresAPI:
    @allure.id("023")
    @allure.tag("API")
    @allure.story("Получение списка жанров")
    @allure.title("Получение списка жанров")
    @pytest.mark.smoke
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_get_genres_list(self, api_client_factory):
        client = api_client_factory(GenreClient)

        response = await client.get_genres_list()
        assert response.status_code == 200

        genres = response.parse_to(List[IdNameResponse])
        assert isinstance(genres, list), "Ответ не является списком list"
        assert len(genres) > 0, "В ответе получен пустой список"

    @allure.id("024")
    @allure.tag("API")
    @allure.story("Добавление жанра")
    @allure.title("Успешное добавление нового жанра с валидными данными")
    @pytest.mark.smoke
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_add_genre(self, api_client_factory):
        client = api_client_factory.authorized(GenreClient)

        genre_name = "Фэнтези"
        genre_to_add = IdNameResponse(Id=uuid.uuid4(), Name=genre_name)
        genre = None

        try:
            with allure.step("Добавление жанра"):
                response_added_genre = await client.add_genre(genre_to_add)
                assert response_added_genre.status_code == 200

                genre = response_added_genre.parse_to(IdNameResponse)
                assert genre.name == genre_to_add.name

        finally:
            if genre:
                with allure.step("Удаление добавленного жанра"):
                    response_deleted_genre = await client.delete_genre(genre.id)
                    assert response_deleted_genre.status_code == 200

                    deleted_genre = response_deleted_genre.parse_to(bool)
                    assert deleted_genre == True

    @allure.id("025")
    @allure.tag("API")
    @allure.story("Добавление жанра")
    @allure.title("Попытка создания дубликата жанра")
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_add_genre_duplicate(self, api_client_factory):
        client = api_client_factory.authorized(GenreClient)

        genre_name = "TestGenre"
        genre_to_add = IdNameResponse(Id=uuid.uuid4(), Name=genre_name)
        genre = None

        try:
            with allure.step("Добавление жанра"):
                response_added_genre = await client.add_genre(genre_to_add)
                assert response_added_genre.status_code == 200

                genre = response_added_genre.parse_to(IdNameResponse)
                assert genre.name == genre_to_add.name

                with allure.step("Добавление дубликата жанра"):
                    response_added_genre = await client.add_genre(genre_to_add)
                    assert response_added_genre.status_code == 400

        finally:
            if genre:
                with allure.step("Удаление добавленного жанра"):
                    response_deleted_genre = await client.delete_genre(genre.id)
                    assert response_deleted_genre.status_code == 200

                    deleted_genre = response_deleted_genre.parse_to(bool)
                    assert deleted_genre == True

    @allure.id("026")
    @allure.tag("API")
    @allure.story("Разграничение ролей")
    @allure.title(
        "Анонимный пользователь не имеет доступа к функционалу добавлению жанра"
    )
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_add_genre_unauthorized_user(self, api_client_factory):
        client = api_client_factory(GenreClient)

        genre_name = "TestGenre"
        genre_to_add = IdNameResponse(Id=uuid.uuid4(), Name=genre_name)

        response_added_genre = await client.add_genre(genre_to_add)
        assert response_added_genre.status_code == 401

    @allure.id("027")
    @allure.tag("API")
    @allure.story("Добавление жанра")
    @allure.title("Попытка создания жанра с невалидным именем")
    @pytest.mark.regress
    @pytest.mark.asyncio
    @pytest.mark.parametrize("invalid_name", ["", "   ", "A" * 256])
    async def test_add_genre_invalid_name(self, api_client_factory, invalid_name):
        client = api_client_factory.authorized(GenreClient)

        genre_to_add = IdNameResponse(Id=uuid.uuid4(), Name=invalid_name)

        with allure.step(f"Добавление жанра с именем: '{invalid_name}'"):
            response = await client.add_genre(genre_to_add)
            assert response.status_code == 400

    @allure.id("028")
    @allure.tag("API")
    @allure.story("Редактирование жанра")
    @allure.title("Успешное редактирование нового жанра с валидными данными")
    @pytest.mark.smoke
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_update_genre(self, api_client_factory):
        client = api_client_factory.authorized(GenreClient)
        genre_updated = None

        with allure.step("Получение жанра из списка жанров"):
            response_list = await client.get_genres_list()
            assert response_list.status_code == 200

            genres = response_list.parse_to(List[IdNameResponse])
            assert isinstance(genres, list), "Ответ не является списком list"
            assert len(genres) > 0, "В ответе получен пустой список"

            genre_to_update = genres[0]
            original_name = genre_to_update.name
            genre_to_update.name = "TestGenre"

        try:
            with allure.step("Обновление названия жанра"):
                response_update = await client.update_genre(
                    genre_to_update.id, genre_to_update
                )
                assert response_update.status_code == 200

                genre_updated = response_update.parse_to(IdNameResponse)
                assert genre_updated.name == genre_to_update.name

        finally:
            if genre_updated:
                with allure.step("Восстановление названия жанра"):
                    genre_to_update.name = original_name
                    response_update = await client.update_genre(
                        genre_to_update.id, genre_to_update
                    )
                    assert response_update.status_code == 200

                    response_list = await client.get_genres_list()
                    assert response_list.status_code == 200

                    genres = response_list.parse_to(List[IdNameResponse])

                    fresh_genre = next(g for g in genres if g.id == genre_to_update.id)
                    assert fresh_genre.name == original_name

    @allure.id("029")
    @allure.tag("API")
    @allure.story("Редактирование жанра")
    @allure.title("Попытка изменения имени на уже существующее")
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_update_genre_already_exist(self, api_client_factory):
        client = api_client_factory.authorized(GenreClient)

        with allure.step("Получение жанра из списка жанров"):
            response_list = await client.get_genres_list()
            assert response_list.status_code == 200

            genres = response_list.parse_to(List[IdNameResponse])
            assert isinstance(genres, list), "Ответ не является списком list"
            assert len(genres) >= 2, "Для теста необходимо как минимум 2 жанра"

            genre_to_update1 = genres[0]
            genre_to_update2 = genres[1]

        with allure.step("Попытка переименования первого жанра в имя второго"):
            bad_payload= IdNameResponse(Id=genre_to_update1.id, Name=genre_to_update2.name)

            response_update = await client.update_genre(
                genre_to_update1.id, bad_payload
            )
            assert response_update.status_code == 400
    
        with allure.step("Проверка, что первый жанр не изменился в БД"):
            response_list_after = await client.get_genres_list()
            genres_after = response_list_after.parse_to(List[IdNameResponse])

            fresh_genre = next(g for g in genres_after if g.id == genre_to_update1.id)
            assert fresh_genre.name == genre_to_update1.name

    @allure.id("030")
    @allure.tag("API")
    @allure.story("Редактирование жанра")
    @allure.title("Попытка изменения несуществующего жанра")
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_update_non_existent_genre(self, api_client_factory):
        client = api_client_factory.authorized(GenreClient)

        non_existent_id = uuid.uuid4()
        payload= IdNameResponse(Id=non_existent_id, Name="UpdateNonExistent")
        
        with allure.step("Отправка запроса с несуществующим ID жанра"):
            response = await client.update_genre(non_existent_id, payload)
            assert response.status_code == 400

    @allure.id("031")
    @allure.tag("API")
    @allure.story("Удаление жанра")
    @allure.title("Успешное удаление жанра с валидными данными")
    @pytest.mark.smoke
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_delete_genre(self, api_client_factory):
        client = api_client_factory.authorized(GenreClient)
        deleted_genre = None

        with allure.step("Получение жанра из списка жанров"):
            response_list = await client.get_genres_list()
            assert response_list.status_code == 200

            genres = response_list.parse_to(List[IdNameResponse])
            assert isinstance(genres, list), "Ответ не является списком list"
            assert len(genres) > 0, "В ответе получен пустой список"

            genre_to_delete = genres[0]

        try:
            with allure.step("Удаление жанра"):
                response_deleted_genre = await client.delete_genre(genre_to_delete.id)
                assert response_deleted_genre.status_code == 200

                deleted_genre = response_deleted_genre.parse_to(bool)
                assert deleted_genre == True

        finally:
            if deleted_genre:
                with allure.step("Добавление жанра"):
                    response_added_genre = await client.add_genre(genre_to_delete)
                    assert response_added_genre.status_code == 200

                    genre = response_added_genre.parse_to(IdNameResponse)
                    assert genre.name == genre_to_delete.name

    @allure.id("032")
    @allure.tag("API")
    @allure.story("Удаление жанра")
    @allure.title("Попытка удаления несуществующего жанра")
    @pytest.mark.regress
    @pytest.mark.asyncio
    async def test_delete_non_existent_genre(self, api_client_factory):
        client = api_client_factory.authorized(GenreClient)

        non_existent_id = uuid.uuid4()
        
        with allure.step("Отправка запроса с несуществующим ID жанра"):
            response = await client.delete_genre(non_existent_id)
            assert response.status_code == 404