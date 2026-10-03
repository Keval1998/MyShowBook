# Engineering Concepts / Review Questions

## 1. What is CancellationToken?

CancellationToken is a .NET mechanism for cooperative cancellation.

It is not a shared cancellation object for the whole application.

A caller creates or receives a CancellationToken and passes it down through async operations. If cancellation is requested, code that observes the token can stop waiting or stop work and return promptly.

In this project the HTTP request provides the token to the controller. The controller passes it to the helper, and the helper/database utility passes it to MySQL operations.

Typical chain:

HTTP request
-> controller
-> helper
-> database utility
-> MySqlConnector

Why use it here:
- If the client disconnects or the server cancels the request, database/network waits should not continue unnecessarily.
- It keeps async operations cancellable without creating custom cancellation logic.

It does not automatically stop arbitrary CPU code. The called operation must support and observe the token. .NET describes this as cooperative cancellation between the caller and the operation. citeturn9search0turn9search1

## 2. How are seat numbers provided?

The assignment's create-show request explicitly supplies a seats array.

Example:

{
  "name": "friday-night",
  "seats": ["A1", "A2", "A3"],
  "price_paise": 25000
}

Therefore the admin is responsible for providing the seat inventory for that show.

The service does not assume a continuous sequence. An input such as:

["A1", "A3", "A4", "A10"]

is valid and creates exactly those four seats.

This is intentionally simple and assignment-specific. The assignment asks for a show with a supplied list of numbered seats; it does not require a seat-layout generator, row/column model, theatre configuration UI, or automatic filling of missing numbers.

The API validates that:
- at least one seat exists;
- each seat number is non-empty;
- duplicate seat numbers are rejected.

## 3. Why hash the request?

The idempotency key and request hash solve two different problems.

Idempotency key:
- identifies the logical client request;
- lets a retry find the original reservation.

Request hash:
- records what request the key was originally used for;
- detects accidental or malicious reuse of the same key with different seats.

Example:

First request:
idempotency_key = abc
seats = ["A1"]

Retry:
idempotency_key = abc
seats = ["A1"]

The stored hash matches, so the original reservation is returned.

Different request:
idempotency_key = abc
seats = ["A2"]

The hash differs, so the API returns 409 instead of silently changing the meaning of the original request.

The hash is not for password security or for identifying the user. It is a compact deterministic fingerprint of the normalized reservation request.

Seat order is normalized before hashing, so ["A1", "A2"] and ["A2", "A1"] represent the same requested seat set.

## 4. Where is role/user authorization checked?

There are two related checks.

### Authentication

Program.cs configures JWT bearer authentication.

For an incoming request with:

Authorization: Bearer <token>

ASP.NET Core validates the JWT signature, issuer, audience, and expiration. The JWT bearer handler then creates the authenticated ClaimsPrincipal used by the request. citeturn0search3

### User identity

Controllers use:

CurrentUserUtility.GetUserGuid(User)

The User property is the ClaimsPrincipal created by ASP.NET Core authentication.

The reservation user ID therefore comes from the JWT subject claim, not from the JSON body.

### Authorization

[Authorize] on reserve/cancel requires an authenticated user.

[Authorize(Roles = "admin")] on POST /shows requires the authenticated principal to have the admin role.

The login helper creates the JWT with a role claim:
- admin user -> role = admin
- normal user -> role = user

ASP.NET Core's default authorization policy for [Authorize] requires an authenticated user, while role-based authorization can restrict an endpoint to a named role. citeturn0search3turn0search4

The important flow is:

Authorization header
-> JwtBearer authentication
-> validated ClaimsPrincipal
-> [Authorize] / [Authorize(Roles = "admin")]
-> controller User
-> JWT subject used as the reservation identity

No controller accepts user_id from the reservation request body.

## 5. Why does the reservation procedure return more than one result set?

The reservation procedure uses a locking SELECT ... FOR UPDATE to lock the requested seat rows before deciding whether the reservation can proceed.

MySQL releases those locks only when the transaction commits or rolls back. citeturn6search0

The locking SELECT naturally produces a result set. The API consumes that internal result set and then reads the actual outcome result set.

This avoids a cursor and keeps the lock decision inside the database transaction.

MySQL stored procedures can return multiple result sets from multiple SELECT statements, and clients must support consuming them. citeturn5search1

## 6. Why no cursor?

The previous implementation used a cursor to process seats one by one.

That was unnecessary.

The revised procedure:
- supplies all requested seats through a temporary input table;
- locks all matching seat rows with one SELECT ... FOR UPDATE;
- uses COUNT/JOIN queries to validate the set;
- updates all matching seats with one UPDATE;
- inserts all reservation-seat mappings with one INSERT ... SELECT.

This is simpler and keeps the database work set-based.

## 7. Why all-or-nothing for multiple seats?

The assignment requires the behavior to be chosen and documented.

This implementation chooses all-or-nothing.

For ["A1", "A2"], if A1 is available but A2 is already taken, neither seat is booked.

The whole transaction rolls back.

This prevents a client from receiving a partially successful reservation without explicitly designing for partial results.
