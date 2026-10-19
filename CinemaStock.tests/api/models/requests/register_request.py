from pydantic import Field
from api.models.base_models import AppBaseModel


class RegisterRequest(AppBaseModel):
    username:str=Field(alias="Username")
    email: str = Field(alias="Email")
    password: str=Field(alias="Password")