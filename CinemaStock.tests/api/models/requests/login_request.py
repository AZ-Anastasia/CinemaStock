from pydantic import Field
from api.models.base_models import AppBaseModel


class LoginRequest(AppBaseModel):
    email: str=Field(alias="Email")
    password: str=Field(alias="Password")