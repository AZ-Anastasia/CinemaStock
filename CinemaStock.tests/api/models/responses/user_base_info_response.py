from pydantic import Field
from api.models.base_models import AppBaseModel


class UserBaseInfoResponse(AppBaseModel):
    username: str = Field(..., alias="Username")
    email: str = Field(..., alias="Email")