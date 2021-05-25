# 1 API 

| Method  | Path | Remark  |
|---|---|---|
| POST  |  /\<repo\>/objects/\<objectId\> | Write an object |
| GET  |  /\<repo>/objects/\<objectId> |  Get an object  |
| GET  |  /\<repo>/objects/\<objectId>?expected=true | Get an object |
| POST | /\<repo>/refs/\<refname>?deref=true  |  Update an ref |
| GET  | /\<repo>/refs/\<prefix>?deref=true  |  Get all refs |