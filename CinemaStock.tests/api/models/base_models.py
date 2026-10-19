from pydantic import BaseModel


class AppBaseModel(BaseModel):
    model_config={
        # Обращение к полям и по оригинальному имени, и по alias
        "populate_by_name":True,
        "str_strip_whitespace": True
    }