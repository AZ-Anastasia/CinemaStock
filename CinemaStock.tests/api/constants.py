from enum import StrEnum


class HTTPMethod(StrEnum):
    GET="GET",
    POST="POST",
    PUT = "PUT",
    DELETE = "DELETE",
    PATCH="PATCH"