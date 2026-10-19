from typing import Generic, List, Optional, TypeVar
from pydantic import AliasChoices, Field
from api.models.base_models import AppBaseModel

T = TypeVar("T")


class ResultDto(AppBaseModel, Generic[T]):
    is_success: bool = Field(
        validation_alias=AliasChoices("IsSuccess", "isSuccess", "is_success"),
        default=False,
    )
    data: Optional[T] = Field(None, alias="Data")
    errors: Optional[List[str]] | None = Field(alias="Errors", default=None)