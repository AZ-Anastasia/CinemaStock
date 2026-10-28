from typing import Optional
from uuid import UUID
from pydantic import Field
from api.models.base_models import AppBaseModel


class IdNameResponse(AppBaseModel):
    id: UUID = Field(..., alias="Id")
    name: Optional[str] = Field(None, alias="Name")