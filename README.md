# dotnet-aws-s3-storage

.NET 10 microservice for storing files in Amazon S3. It contains no business rules: it provides a stable HTTP API for upload, download, deletion, and temporary URL generation. This lets a Products API, React applications, AWS Lambda, and Node.js APIs use it without knowing the AWS SDK or the bucket organization.

## Amazon S3 in brief

**Amazon S3** is an object storage service. A **Bucket** is the container that holds files. Each file is an **Object**, identified within the bucket by a **Key** (for example, `invoices/2026/a.pdf`). **Metadata** consists of key/value pairs attached to an object; here, the original name is preserved as metadata. A **Presigned URL** is a signed, temporary URL that gives direct access to an object without exposing AWS credentials to the consumer.

## Architecture

```text
Storage.Api -> Storage.Application <- Storage.Infrastructure
                    ^                     |
                    |                     v
               Storage.Domain          Amazon S3
```

- `Storage.Api`: Minimal API, endpoints, middleware, OpenAPI/Scalar, and application composition.
- `Storage.Application`: contracts (`IStorageService` and `IStorageProvider`), DTOs, validation, and storage orchestration. It has no AWS knowledge.
- `Storage.Domain`: only the technical `StorageObject` model; there are no artificial entities or domain rules.
- `Storage.Infrastructure`: the `AwsS3StorageProvider` adapter, AWS SDK client, and `AwsOptions`.
- `Storage.UnitTests`: orchestration and validation tests, without real AWS.

`IStorageService` is the contract consumed by the API and keeps the HTTP boundary simple. `IStorageProvider` applies dependency inversion: a future implementation for Azure Blob, MinIO, or Google Cloud would replace only Infrastructure, without changing Application.

Intentional decisions: there is no database because S3 is already the storage source; there is no CQRS/MediatR because the operations are direct. Application owns `StorageKeyBuilder`, the sole point that composes the upload key; Infrastructure receives the finished key and only sends it to S3. The provider streams data to avoid loading entire files into memory.

## Folders are prefixes

Amazon S3 has no physical directories. A "folder" is only the prefix of an object key. For example, `products/images/notebook.png` is a single key; the S3 console merely displays it as a visual hierarchy.

During upload, the client sends `file` and optionally `folder`; it neither sends nor builds the key. Application normalizes the folder by removing leading, trailing, and repeated slashes, then builds the key in one place:

| Submitted folder | File | Returned key |
|---|---|---|
| `products` | `notebook.png` | `products/notebook.png` |
| `products/` | `notebook.png` | `products/notebook.png` |
| `/products/images/` | `notebook.png` | `products/images/notebook.png` |
| empty or omitted | `notebook.png` | `notebook.png` |

The folder can be up to 200 characters long and accepts only letters, numbers, `-`, `_`, and `/`. To prevent ambiguous keys or traversal, `..`, `//`, `\\`, and characters outside this list are rejected. `StorageKeyBuilder` normalization also ensures that no generated key contains duplicate slashes.

## Endpoints

| Method | Route | Description |
|---|---|---|
| POST | `/files/upload` | Sends `multipart/form-data` with `file` and optional `folder` (max. 10 MB). |
| GET | `/files/{key}` | Downloads the file. |
| DELETE | `/files/{key}` | Deletes the object. |
| GET | `/files/{key}/presigned-url` | Creates a download URL valid for 15 minutes. |
| GET | `/health/live` | Liveness check, without an S3 call. |
| GET | `/files` | Lists files, with pagination and an optional prefix filter. |
| PUT | `/files/rename` | Renames through copy followed by deletion. |

When enabled, the OpenAPI document is at `/openapi/v1.json` and the Scalar UI is at `/scalar`. Control it with `OpenApi__Enabled=false`; it is independent of the Development environment.

## Set up AWS

1. In the S3 console, create a bucket in a region (for example, `us-east-1`). Keep its name.
2. In IAM, create a user or, preferably in production, a role with programmatic access limited to the bucket. Grant `s3:PutObject`, `s3:GetObject`, and `s3:DeleteObject` on `arn:aws:s3:::YOUR_BUCKET/*`.
3. Under **Security credentials**, create an Access Key and copy the Access Key ID and Secret Access Key only once.

Never commit keys to the repository. In EC2, ECS, or Lambda, leave `AccessKey` and `SecretKey` empty so the AWS SDK uses the default credential/role chain.

## Configuration and running

For local development, use User Secrets:

```powershell
dotnet user-secrets init --project src/Storage.Api
dotnet user-secrets set "Aws:BucketName" "your-bucket" --project src/Storage.Api
dotnet user-secrets set "Aws:Region" "us-east-1" --project src/Storage.Api
dotnet user-secrets set "Aws:AccessKey" "your-access-key" --project src/Storage.Api
dotnet user-secrets set "Aws:SecretKey" "your-secret-key" --project src/Storage.Api
dotnet run --project src/Storage.Api
```

Environment variables are also accepted, for example `Aws__BucketName`, `Aws__Region`, `Aws__AccessKey`, and `Aws__SecretKey`. `appsettings.json` contains only empty example values.

Upload example:

```powershell
curl.exe -F "file=@C:\temp\notebook.png" -F "folder=products/images" http://localhost:5000/files/upload
```

The upload response provides the full key, normalized folder, file name, size, and content type:

```json
{
  "key": "products/images/notebook.png",
  "folder": "products/images",
  "fileName": "notebook.png",
  "size": 12345,
  "contentType": "image/png"
}
```

For Docker (without LocalStack, using real AWS), set `AWS_BUCKET_NAME`, `AWS_REGION`, `AWS_ACCESS_KEY_ID`, and `AWS_SECRET_ACCESS_KEY`, then run:

```powershell
docker compose up --build
```

## Listing files and pagination

Use `GET /files` to retrieve the first page. `pageSize` is optional (default 20, maximum 1000), and `prefix` limits results to keys that start with the supplied value:

```powershell
curl.exe "http://localhost:5000/files?pageSize=20&prefix=products/"
```

Amazon S3 paginates object listings. When `hasMore` is `true`, send the returned `nextContinuationToken` value in the next request. The **Continuation Token** is an opaque cursor generated by S3: the client must not interpret or alter it.

```powershell
curl.exe "http://localhost:5000/files?pageSize=20&continuationToken=RECEIVED_TOKEN"
```

Each item returns only `key`, `size`, `lastModified`, `contentType` (when available), and `storageClass`.

## Renaming files

S3 has no native rename operation. Therefore, `PUT /files/rename` performs:

```text
CopyObject
   ↓
DeleteObject
```

```powershell
curl.exe -X PUT http://localhost:5000/files/rename -H "Content-Type: application/json" -d '{"oldKey":"products/old.pdf","newKey":"products/new.pdf"}'
```

If the source does not exist, the API returns `404`; if the destination already exists, it returns `409`.

## Tests

```powershell
dotnet test DotnetAwsS3Storage.slnx
```

The tests cover `StorageKeyBuilder`, folder rules, `StorageService` mapping, and required-file, required-name, and maximum-size rules. The API uses Serilog and a correlation ID; logs do not record the Access Key or Secret Key.