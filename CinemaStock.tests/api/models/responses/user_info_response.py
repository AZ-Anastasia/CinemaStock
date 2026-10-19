from typing import List
from pydantic import Field
from api.models.base_models import AppBaseModel


class UserInfoResponse(AppBaseModel):
    id: str = Field(..., alias="Id")
    username: str = Field(..., alias="Username")
    email: str = Field(..., alias="Email")
    roles: List[str] = Field(..., alias="Roles")