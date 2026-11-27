# Exam Planner API Contract
 
**Version:** 1.0  
**Status:** Draft / team-review baseline  
**API base path:** `/api/v1`  
**Primary transport:** HTTPS  
**Default data format:** JSON (`application/json`)  
**Authentication:** JWT Bearer token  
**Backend:** C# / ASP.NET Core  
**Frontend:** JavaScript

---

## 1. Purpose

This document defines the contract between the Exam Planner frontend and backend.

It describes:

- which HTTP endpoints the frontend may call;
- which request fields the backend accepts;
- which response fields the backend returns;
- authentication and authorization expectations;
- standard error responses;
- file upload/download behavior;
- scheduling and conflict-checking operations.

The contract should be updated whenever the frontend/backend interface changes. Database implementation details are intentionally hidden behind the API.

---

## 2. Scope and source basis

Version 1 is based on the current project requirements, use cases, backlog, core-planning notes, diagrams, and product-owner/admin clarifications.

The contract currently covers:

1. Login.
2. Importing exam-planning data.
3. Reading classes, students, exams, and lecturers.
4. Creating/generating schedules.
5. Detecting student and examiner conflicts.
6. Rescheduling exams.
7. Assigning students to exams, including extra-time information.
8. Checking re-exam attempt eligibility.
9. Assigning examiners.
10. Managing lecturer constraints.
11. Approving a schedule.
12. Exporting an anonymized schedule.
13. Preserving schedule history/version information where supported.

The following project decisions are **not fully settled** and are therefore marked `TBD` where relevant:

- whether teachers/lecturers have their own authenticated accounts in Version 1;
- whether students ever log in;
- the exact meaning/scope of the “maximum 3 exams per week” rule;
- the email provider/protocol;
- room-planning endpoints;
- whether schedule generation/rebuilding is fully automatic in the first implementation;
- the exact accepted import file variants beyond the Excel source used by the current process.

---

## 3. General conventions

### 3.1 Base URL

All API endpoints use the relative base path:

```text
/api/v1
```

Example:

```text
POST /api/v1/auth/login
```

The host and port depend on the environment and are not part of this contract.

### 3.2 Authentication header

Protected endpoints require:

```http
Authorization: Bearer <access-token>
```

The login endpoint is public.

### 3.3 JSON naming

JSON fields use `camelCase`.

Example:

```json
{
  "examDate": "2027-01-18",
  "sessionType": "Ordinary"
}
```

### 3.4 Date and time formats

| Value | Format | Example |
|---|---|---|
| Date | `YYYY-MM-DD` | `2027-01-18` |
| Time | `HH:mm:ss` | `13:00:00` |
| Timestamp | ISO 8601 UTC | `2027-01-18T12:00:00Z` |

### 3.5 IDs

Internal resource IDs are integers unless the implementation later requires another representation.

Examples:

```text
studentId = 15
examId = 42
scheduleId = 7
```

The student's VIA ID is **not** the same as the internal `studentId`.

### 3.6 Boolean values

Boolean values are JSON `true` or `false`.

### 3.7 Standard success status codes

| Status | Meaning |
|---|---|
| `200 OK` | Request completed successfully |
| `201 Created` | A resource was created |
| `204 No Content` | Request succeeded and no response body is needed |

### 3.8 Standard error status codes

| Status | Meaning |
|---|---|
| `400 Bad Request` | Invalid request format or invalid field value |
| `401 Unauthorized` | Missing/invalid login credentials or JWT |
| `403 Forbidden` | Authenticated user lacks permission |
| `404 Not Found` | Requested resource does not exist |
| `409 Conflict` | Request violates a scheduling/business rule |
| `415 Unsupported Media Type` | Unsupported upload file type |
| `422 Unprocessable Content` | Request/file is readable but its data is invalid |
| `500 Internal Server Error` | Unexpected backend failure |

---

## 4. Standard error response

All JSON error responses should use the same structure.

```json
{
  "code": "STUDENT_OVERLAP",
  "message": "The selected date creates a student exam conflict.",
  "details": {}
}
```

### Fields

| Field | Type | Required | Meaning |
|---|---|---:|---|
| `code` | string | Yes | Machine-readable error code |
| `message` | string | Yes | Human-readable explanation |
| `details` | object | No | Additional structured information |

The frontend should make decisions using `code`, not by parsing the text in `message`.

---

# 5. Authentication

## 5.1 Login

### `POST /api/v1/auth/login`

Authenticates a user with VIA email/password credentials and returns a JWT access token.

**Authentication required:** No

### Request body

```json
{
  "email": "admin@via.dk",
  "password": "example-password"
}
```

| Field | Type | Required | Meaning |
|---|---|---:|---|
| `email` | string | Yes | VIA email address |
| `password` | string | Yes | Password supplied for authentication |

### `200 OK`

```json
{
  "accessToken": "<jwt-token>",
  "expiresAt": "2026-09-14T16:00:00Z",
  "user": {
    "id": 1,
    "email": "admin@via.dk",
    "role": "Admin"
  }
}
```

### Errors

| Status | Code | Meaning |
|---|---|---|
| `400` | `INVALID_LOGIN_REQUEST` | Email/password format is invalid |
| `401` | `INVALID_CREDENTIALS` | Email/password is incorrect |

### Notes

- Passwords must not be returned by the API.
- If the project stores credentials locally, passwords must not be stored as plain text.
- Current use-case documentation explicitly requires Admin login.
- Backlog/project notes also refer to Teacher access, but the final authentication model for Teacher is still `TBD`.

---

## 5.2 Current user

### `GET /api/v1/auth/me`

Returns the authenticated user's basic identity/role so the frontend can restore the session after a page reload.

**Authentication required:** Yes

### `200 OK`

```json
{
  "id": 1,
  "email": "admin@via.dk",
  "role": "Admin"
}
```

### Errors

| Status | Code |
|---|---|
| `401` | `UNAUTHORIZED` |

---

# 6. Importing data

## 6.1 Import planning data

### `POST /api/v1/imports`

Uploads an exam-planning file for validation and import.

**Authentication required:** Yes  
**Required role:** Admin  
**Content-Type:** `multipart/form-data`

### Form data

```text
file = exam-planning.xlsx
```

### `200 OK`

```json
{
  "importId": 12,
  "success": true,
  "classesImported": 223,
  "studentsImported": 814,
  "examsImported": 223,
  "warnings": [
    "2 rows contained missing lecturer information."
  ]
}
```

### Errors

| Status | Code | Meaning |
|---|---|---|
| `400` | `FILE_REQUIRED` | No file supplied |
| `415` | `UNSUPPORTED_FILE_TYPE` | File format is not supported |
| `422` | `INVALID_IMPORT_DATA` | File can be read but data is invalid/incomplete |
| `500` | `IMPORT_FAILED` | Import failed unexpectedly |

### Notes

- The frontend does not interact directly with the upload database or planning database.
- The current process is based on Excel exports. Additional accepted formats are `TBD`.
- Old classes must remain importable/available when students still require re-exams.

---

# 7. Classes

## 7.1 Get all classes

### `GET /api/v1/classes`

Returns classes available to the planning system, including older classes that remain relevant for re-exams.

**Authentication required:** Yes

### Optional query parameters

| Parameter | Type | Meaning |
|---|---|---|
| `courseId` | integer | Filter by course |
| `semester` | string | Filter by semester |
| `prefix` | string | Filter by programme prefix |
| `includeEnded` | boolean | Include classes past their teaching end date |

### `200 OK`

```json
[
  {
    "id": 18,
    "courseId": 4,
    "classCode": "IT-BPR2-A25",
    "nickname": "BPR2",
    "prefix": "IT",
    "semester": "7",
    "startDate": "2025-08-18",
    "endDate": "2026-01-30",
    "courseOffering": "Software Technology Engineering - Horsens"
  }
]
```

---

## 7.2 Get one class

### `GET /api/v1/classes/{classId}`

**Authentication required:** Yes

### `200 OK`

Returns one class object in the same format as above.

### Errors

| Status | Code |
|---|---|
| `404` | `CLASS_NOT_FOUND` |

---

# 8. Students

Student names and VIA IDs are privacy-sensitive data. They must never appear in the anonymized export.

## 8.1 Get students

### `GET /api/v1/students`

**Authentication required:** Yes  
**Recommended permission:** Admin/planning access

### Optional query parameters

| Parameter | Type | Meaning |
|---|---|---|
| `classId` | integer | Students enrolled in a class |
| `examId` | integer | Students assigned to an exam |

### `200 OK`

```json
[
  {
    "id": 15,
    "viaId": "123456",
    "name": "Example Student"
  }
]
```

---

## 8.2 Get one student

### `GET /api/v1/students/{studentId}`

**Authentication required:** Yes

### `200 OK`

```json
{
  "id": 15,
  "viaId": "123456",
  "name": "Example Student"
}
```

### Errors

| Status | Code |
|---|---|
| `404` | `STUDENT_NOT_FOUND` |

---

# 9. Exams

## 9.1 Get exams

### `GET /api/v1/exams`

Returns exams available for planning.

**Authentication required:** Yes

### Optional query parameters

| Parameter | Type | Meaning |
|---|---|---|
| `classId` | integer | Exams linked to a class |
| `priority` | integer | Filter by priority |
| `city` | string | Filter by city/campus |

### `200 OK`

```json
[
  {
    "id": 42,
    "gradingScale": "7-point scale",
    "examinerType": "External",
    "examFormat": "Oral",
    "prerequisites": "Project submitted",
    "duration": "20 min. per student",
    "planningResponsible": "ABC",
    "city": "Horsens",
    "remarks": "",
    "priority": 3,
    "classes": [
      {
        "id": 18,
        "classCode": "IT-BPR2-A25"
      }
    ]
  }
]
```

---

## 9.2 Get one exam

### `GET /api/v1/exams/{examId}`

**Authentication required:** Yes

### `200 OK`

Returns one exam object in the same format as above.

### Errors

| Status | Code |
|---|---|
| `404` | `EXAM_NOT_FOUND` |

---

# 10. Student exam assignment and re-exam eligibility

A student's assignment to an exam is separate from the student's class enrollment and from exam hand-in deadlines.

The backend is responsible for checking whether the student still has an available exam attempt.

## 10.1 Get student eligibility for an exam

### `GET /api/v1/exams/{examId}/students/{studentId}/eligibility`

Checks whether a student may currently be assigned to the exam/re-exam.

**Authentication required:** Yes  
**Required role:** Admin

### `200 OK`

```json
{
  "studentId": 15,
  "examId": 42,
  "eligible": true,
  "attemptsUsed": 2,
  "maxAttempts": 3,
  "remainingAttempts": 1,
  "reason": null
}
```

Example when no attempt remains:

```json
{
  "studentId": 15,
  "examId": 42,
  "eligible": false,
  "attemptsUsed": 3,
  "maxAttempts": 3,
  "remainingAttempts": 0,
  "reason": "NO_REMAINING_ATTEMPTS"
}
```

### Business-rule note

- A student normally has three attempts.
- An approved additional attempt may raise the permitted maximum (for example to four).
- An accepted illness absence does not consume an attempt.
- The API stores/returns the resulting attempt state; it does not need to store medical details unless the project scope is later expanded.

---

## 10.2 Assign student to exam

### `PUT /api/v1/exams/{examId}/students/{studentId}`

Assigns a student to an exam and stores whether that student requires extra time for that exam.

**Authentication required:** Yes  
**Required role:** Admin

### Request body

```json
{
  "extraTime": true
}
```

### `200 OK`

```json
{
  "studentId": 15,
  "examId": 42,
  "extraTime": true,
  "attemptsUsed": 2,
  "maxAttempts": 3,
  "remainingAttempts": 1,
  "eligible": true
}
```

### Errors

| Status | Code | Meaning |
|---|---|---|
| `404` | `STUDENT_NOT_FOUND` | Student does not exist |
| `404` | `EXAM_NOT_FOUND` | Exam does not exist |
| `409` | `NO_REMAINING_ATTEMPTS` | Student has used all permitted attempts |
| `409` | `PREREQUISITES_NOT_MET` | Student fails a required eligibility rule, if implemented |

---

## 10.3 Remove student from exam

### `DELETE /api/v1/exams/{examId}/students/{studentId}`

**Authentication required:** Yes  
**Required role:** Admin

### `204 No Content`

---

# 11. Lecturers and examiner assignments

## 11.1 Get lecturers

### `GET /api/v1/lecturers`

**Authentication required:** Yes

### `200 OK`

```json
[
  {
    "id": 8,
    "initials": "ABC"
  }
]
```

---

## 11.2 Get one lecturer

### `GET /api/v1/lecturers/{lecturerId}`

**Authentication required:** Yes

### `200 OK`

```json
{
  "id": 8,
  "initials": "ABC"
}
```

---

## 11.3 Assign examiner to exam

### `PUT /api/v1/exams/{examId}/examiners/{lecturerId}`

Assigns a lecturer/examiner to an exam.

**Authentication required:** Yes  
**Required role:** Admin

### Request body

```json
{
  "role": "InternalExaminer"
}
```

### `200 OK`

```json
{
  "examId": 42,
  "lecturerId": 8,
  "role": "InternalExaminer"
}
```

### Errors

| Status | Code |
|---|---|
| `404` | `EXAM_NOT_FOUND` |
| `404` | `LECTURER_NOT_FOUND` |

---

## 11.4 Remove examiner from exam

### `DELETE /api/v1/exams/{examId}/examiners/{lecturerId}`

**Authentication required:** Yes  
**Required role:** Admin

### `204 No Content`

---

# 12. Lecturer constraints

Lecturer constraints represent dates on which a lecturer is unavailable or has another planning preference.

## 12.1 Get lecturer constraints

### `GET /api/v1/lecturers/{lecturerId}/constraints`

**Authentication required:** Yes

### `200 OK`

```json
[
  {
    "id": 73,
    "constraintDate": "2027-01-14",
    "constraintType": "Unavailable",
    "note": "Unavailable this day"
  }
]
```

---

## 12.2 Create lecturer constraint

### `POST /api/v1/lecturers/{lecturerId}/constraints`

**Authentication required:** Yes

### Request body

```json
{
  "constraintDate": "2027-01-14",
  "constraintType": "Unavailable",
  "note": "Unavailable this day"
}
```

### `201 Created`

```json
{
  "id": 73,
  "lecturerId": 8,
  "constraintDate": "2027-01-14",
  "constraintType": "Unavailable",
  "note": "Unavailable this day"
}
```

### Notes

The exact allowed values for `constraintType` are still subject to team/PO agreement. A reasonable initial set is:

```text
Unavailable
Preferred
Required
```

The core-planning notes also distinguish must-respect and nice-to-have constraints. The final enum should be agreed before implementation.

---

## 12.3 Update lecturer constraint

### `PATCH /api/v1/lecturers/{lecturerId}/constraints/{constraintId}`

**Authentication required:** Yes

Example request:

```json
{
  "constraintType": "Preferred",
  "note": "Prefer another date if possible"
}
```

### `200 OK`

Returns the updated constraint.

---

## 12.4 Delete lecturer constraint

### `DELETE /api/v1/lecturers/{lecturerId}/constraints/{constraintId}`

**Authentication required:** Yes

### `204 No Content`

---

# 13. Schedule creation and generation

## 13.1 Create/generate schedule

### `POST /api/v1/schedules`

Creates a new exam schedule using the current imported planning data.

**Authentication required:** Yes  
**Required role:** Admin

### Request body

```json
{
  "name": "January 2027 Exam Plan",
  "examDays": [
    {
      "date": "2027-01-04",
      "type": "Ordinary",
      "usable": true
    },
    {
      "date": "2027-01-05",
      "type": "Ordinary",
      "usable": true
    },
    {
      "date": "2027-02-10",
      "type": "ReExam",
      "usable": true
    }
  ]
}
```

### `201 Created`

```json
{
  "id": 7,
  "name": "January 2027 Exam Plan",
  "approved": false,
  "version": 1,
  "sessions": [
    {
      "id": 31,
      "examId": 42,
      "examDate": "2027-01-14",
      "sessionType": "Ordinary",
      "locked": false,
      "note": ""
    }
  ],
  "unplacedExams": []
}
```

### Notes

The scheduling engine should take the project's core rules into account, including where implemented:

- student clashes;
- examiner/lecturer clashes;
- lecturer constraints;
- priority order;
- co-taught courses that must share a date;
- usable exam days;
- locked sessions;
- exam-day capacity rules;
- multi-day exams.

Not every rule needs to be implemented in the first sprint, but the frontend contract should not require direct access to the algorithm.

---

## 13.2 List schedules

### `GET /api/v1/schedules`

**Authentication required:** Yes

### `200 OK`

```json
[
  {
    "id": 7,
    "name": "January 2027 Exam Plan",
    "approved": false,
    "version": 1
  }
]
```

---

## 13.3 Get current schedule

### `GET /api/v1/schedules/current`

Returns the schedule considered current by the system.

**Authentication required:** Yes

### `200 OK`

Returns a full schedule object.

### Errors

| Status | Code |
|---|---|
| `404` | `NO_CURRENT_SCHEDULE` |

---

## 13.4 Get schedule

### `GET /api/v1/schedules/{scheduleId}`

**Authentication required:** Yes

### `200 OK`

```json
{
  "id": 7,
  "name": "January 2027 Exam Plan",
  "approved": false,
  "version": 1,
  "sessions": [
    {
      "id": 31,
      "examId": 42,
      "examDate": "2027-01-14",
      "sessionType": "Ordinary",
      "locked": false,
      "note": ""
    }
  ],
  "unplacedExams": []
}
```

---

# 14. Conflict checking

## 14.1 Scan schedule for conflicts

### `GET /api/v1/schedules/{scheduleId}/conflicts`

Checks the schedule for conflicts without changing it.

**Authentication required:** Yes

### `200 OK`

```json
{
  "hasConflicts": true,
  "conflicts": [
    {
      "type": "STUDENT_OVERLAP",
      "date": "2027-01-14",
      "examIds": [42, 67],
      "affectedStudentIds": [15, 91],
      "affectedCount": 2
    },
    {
      "type": "EXAMINER_OVERLAP",
      "date": "2027-01-16",
      "examIds": [31, 44],
      "lecturerIds": [8],
      "affectedCount": 1
    }
  ]
}
```

### Initial conflict codes

```text
STUDENT_OVERLAP
EXAMINER_OVERLAP
LECTURER_UNAVAILABLE
CAMPUS_CONFLICT
WEEKLY_EXAM_LIMIT
```

`WEEKLY_EXAM_LIMIT` is included because it appears in the approval use case, but its exact scope (for example per student vs. another interpretation) is `TBD` and must be clarified before enforcing it.

---

# 15. Rescheduling exam sessions

## 15.1 Move an exam session

### `PATCH /api/v1/schedules/{scheduleId}/sessions/{sessionId}`

Changes an existing exam session, most commonly its date.

**Authentication required:** Yes  
**Required role:** Admin

### Request body

```json
{
  "examDate": "2027-01-18"
}
```

### `200 OK`

```json
{
  "id": 31,
  "examId": 42,
  "examDate": "2027-01-18",
  "sessionType": "Ordinary",
  "locked": false,
  "note": ""
}
```

### Conflict response: `409 Conflict`

```json
{
  "code": "STUDENT_OVERLAP",
  "message": "The selected date creates a student exam conflict.",
  "details": {
    "conflictingExamIds": [67],
    "affectedCount": 3
  }
}
```

### Notification side effect

If automatic schedule-update emails are enabled, a successful schedule modification may trigger the backend notification mechanism. The frontend does not need to call an email endpoint directly.

---

# 16. Locking exam sessions

The core-planning notes refer to exams whose date is already decided and must not be moved automatically.

## 16.1 Change lock state

### `PATCH /api/v1/schedules/{scheduleId}/sessions/{sessionId}/lock`

**Authentication required:** Yes  
**Required role:** Admin

### Request body

```json
{
  "locked": true
}
```

### `200 OK`

```json
{
  "sessionId": 31,
  "locked": true
}
```

This endpoint may be deferred if lock/unlock is not part of the first frontend implementation.

---

# 17. Rebuilding a schedule after changes

The core-planning notes specify that late constraints should allow affected exams to be replanned while preserving already approved/locked work.

## 17.1 Rebuild affected schedule

### `POST /api/v1/schedules/{scheduleId}/rebuild`

**Authentication required:** Yes  
**Required role:** Admin

### Request body

```json
{
  "affectedExamIds": [42, 67]
}
```

If `affectedExamIds` is omitted, the backend may determine the affected set itself.

### `200 OK`

```json
{
  "scheduleId": 7,
  "previousVersion": 1,
  "newVersion": 2,
  "movedSessions": [31],
  "unplacedExams": []
}
```

This endpoint is part of the planned core logic and may be deferred until schedule regeneration is implemented.

---

# 18. Schedule approval

## 18.1 Approve schedule

### `POST /api/v1/schedules/{scheduleId}/approve`

Approves a schedule after the required checks have been performed.

**Authentication required:** Yes

**Authorization:** Requires schedule-approval permission. The use-case actor is currently called `Teacher`; exact authentication/role mapping is `TBD`.

### Request body

Nobody is required initially.

### `200 OK`

```json
{
  "id": 7,
  "approved": true,
  "approvedAt": "2027-01-02T12:00:00Z"
}
```

### Errors

| Status | Code | Meaning |
|---|---|---|
| `404` | `SCHEDULE_NOT_FOUND` | Schedule does not exist |
| `409` | `SCHEDULE_HAS_CONFLICTS` | Blocking conflicts remain |

Example conflict response:

```json
{
  "code": "SCHEDULE_HAS_CONFLICTS",
  "message": "The schedule cannot be approved while blocking conflicts remain.",
  "details": {
    "conflictCount": 3
  }
}
```

---

# 19. Schedule history and versions

Project/core-logic notes state that previous plans should be preserved when a plan is rebuilt.

## 19.1 Get schedule versions

### `GET /api/v1/schedules/{scheduleId}/versions`

**Authentication required:** Yes

### `200 OK`

```json
[
  {
    "version": 1,
    "createdAt": "2027-01-02T09:00:00Z"
  },
  {
    "version": 2,
    "createdAt": "2027-01-03T10:15:00Z"
  }
]
```

---

## 19.2 Get a specific version

### `GET /api/v1/schedules/{scheduleId}/versions/{version}`

**Authentication required:** Yes

Returns a read-only snapshot of that schedule version.

This version-history API may be deferred if history storage is not part of the first implementation sprint.

---

# 20. Exporting an anonymized schedule

## 20.1 Export schedule

### `GET /api/v1/schedules/{scheduleId}/export?format=xlsx`

Exports a schedule after anonymizing student names and VIA IDs.

**Authentication required:** Yes  
**Required role:** Admin

### Successful response

```http
200 OK
Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
Content-Disposition: attachment; filename="exam-schedule-anonymized.xlsx"
```

The response body is the generated file.

### Errors

| Status | Code | Meaning |
|---|---|---|
| `404` | `SCHEDULE_NOT_FOUND` | No such schedule |
| `409` | `ANONYMIZATION_FAILED` | File must not be downloaded if anonymization fails |
| `500` | `EXPORT_FAILED` | Export generation failed |

### Privacy rule

Student `name` and `viaId` must not be included in the exported external schedule.

Anonymization must be performed by the backend before the file is returned. The frontend must not receive sensitive data and merely hide it visually.

---

# 21. Automatic email notifications

The project requirement states that the system automatically sends an email when the exam plan is updated.

This is primarily a backend side effect and therefore does **not** require a public frontend endpoint in Version 1.

Examples of operations that may trigger notifications:

- successful exam rescheduling;
- schedule rebuild;
- schedule approval;
- other PO-approved schedule changes.

The exact email provider/protocol and recipient rules are `TBD`.

If manual resend functionality is requested later, it should be added as a separate endpoint instead of exposing the internal email service directly.

---

# 22. Hand-in data

The current diagrams model exam hand-ins separately because an exam can have multiple deadlines/parts.

The current frontend requirements do not explicitly require CRUD screens for hand-ins, so no mandatory Version 1 write endpoints are defined here.

If the frontend needs this data, the following read endpoint is recommended:

### `GET /api/v1/exams/{examId}/hand-ins`

Example response:

```json
[
  {
    "id": 91,
    "handInDate": "2026-12-19",
    "handInTime": "13:00:00",
    "part": "Part 1",
    "note": ""
  }
]
```

Write endpoints can be added when a requirement explicitly asks users to create/edit hand-in deadlines.

---

# 23. Frontend service/function contract

The frontend may wrap the HTTP endpoints in functions similar to the following TypeScript-style signatures.

These are examples of frontend-facing function signatures; they do not force a specific JavaScript framework.

```ts
login(
  email: string,
  password: string
): Promise<LoginResponse>

getCurrentUser(): Promise<AuthUser>

importData(
  file: File
): Promise<ImportResult>

getClasses(): Promise<CourseClass[]>

getStudents(): Promise<Student[]>

getExams(): Promise<Exam[]>

getLecturers(): Promise<Lecturer[]>

getExamEligibility(
  examId: number,
  studentId: number
): Promise<ExamEligibility>

assignStudent(
  examId: number,
  studentId: number,
  extraTime: boolean
): Promise<StudentExamAssignment>

removeStudentFromExam(
  examId: number,
  studentId: number
): Promise<void>

assignExaminer(
  examId: number,
  lecturerId: number,
  role: string
): Promise<ExaminerAssignment>

removeExaminer(
  examId: number,
  lecturerId: number
): Promise<void>

getLecturerConstraints(
  lecturerId: number
): Promise<LecturerConstraint[]>

createLecturerConstraint(
  lecturerId: number,
  request: CreateLecturerConstraintRequest
): Promise<LecturerConstraint>

createSchedule(
  request: CreateScheduleRequest
): Promise<Schedule>

getSchedules(): Promise<ScheduleSummary[]>

getCurrentSchedule(): Promise<Schedule>

getSchedule(
  scheduleId: number
): Promise<Schedule>

scanConflicts(
  scheduleId: number
): Promise<ConflictResult>

rescheduleExam(
  scheduleId: number,
  sessionId: number,
  newDate: string
): Promise<ExamSession>

setSessionLocked(
  scheduleId: number,
  sessionId: number,
  locked: boolean
): Promise<SessionLockResult>

rebuildSchedule(
  scheduleId: number,
  affectedExamIds?: number[]
): Promise<ScheduleRebuildResult>

approveSchedule(
  scheduleId: number
): Promise<ScheduleApprovalResult>

exportSchedule(
  scheduleId: number
): Promise<Blob>
```

---

# 24. Suggested DTO shapes

These are shared shapes that frontend and backend should agree on. They may be implemented with C# DTO classes on the backend and JavaScript/TypeScript types on the frontend.

## 24.1 `AuthUser`

```json
{
  "id": 1,
  "email": "admin@via.dk",
  "role": "Admin"
}
```

## 24.2 `Student`

```json
{
  "id": 15,
  "viaId": "123456",
  "name": "Example Student"
}
```

## 24.3 `CourseClass`

```json
{
  "id": 18,
  "courseId": 4,
  "classCode": "IT-BPR2-A25",
  "nickname": "BPR2",
  "prefix": "IT",
  "semester": "7",
  "startDate": "2025-08-18",
  "endDate": "2026-01-30",
  "courseOffering": "Software Technology Engineering - Horsens"
}
```

## 24.4 `ExamSession`

```json
{
  "id": 31,
  "examId": 42,
  "examDate": "2027-01-18",
  "sessionType": "Ordinary",
  "locked": false,
  "note": ""
}
```

## 24.5 `StudentExamAssignment`

```json
{
  "studentId": 15,
  "examId": 42,
  "extraTime": true,
  "attemptsUsed": 2,
  "maxAttempts": 3,
  "remainingAttempts": 1,
  "eligible": true
}
```

## 24.6 `ExaminerAssignment`

```json
{
  "examId": 42,
  "lecturerId": 8,
  "role": "InternalExaminer"
}
```

## 24.7 `ScheduleSummary`

```json
{
  "id": 7,
  "name": "January 2027 Exam Plan",
  "approved": false,
  "version": 1
}
```

## 24.8 `Conflict`

```json
{
  "type": "STUDENT_OVERLAP",
  "date": "2027-01-14",
  "examIds": [42, 67],
  "affectedCount": 2
}
```

---

# 25. Privacy and security rules

1. Passwords must never be returned by the API.
2. Sensitive student identity data (`name`, `viaId`) must be protected by authorization.
3. The anonymized export must not contain student names or VIA IDs.
4. Protected endpoints require a valid JWT.
5. The frontend must not connect directly to PostgreSQL.
6. Database connection strings, signing keys, passwords, and other secrets must not be returned through API responses.
7. Detailed medical documentation is outside the current API contract; attempt eligibility stores the academic result of such decisions, not medical details.

---

# 26. Internal architecture that is not part of the API contract

The project currently considers two PostgreSQL databases: one for uploaded/raw data and one for working/application data.

This is a backend implementation detail. The frontend should not know which database stores a resource and should not receive database-specific information.

The API remains the same if the backend later changes from two databases to one, provided the response/request contract is unchanged.

---

# 27. Explicitly deferred / TBD areas

The following should not be silently invented during implementation. The team/PO should decide them first.

| Topic | Current status |
|---|---|
| Teacher login/authentication | Use cases/backlog reference Teacher access, but exact login model is not final |
| Student login | Project notes mention possible viewing, but current formal login use case is Admin-focused |
| Weekly maximum of 3 exams | Rule exists in approval use case; exact scope needs clarification |
| Room planning | Mentioned in project notes but not yet specified by current use cases/API requirements |
| Teacher messaging | Backlog mentions send/receive/flags; exact workflow/endpoints need clarification |
| Email provider | Requirement exists; provider/protocol not selected |
| Constraint enum values | Must-respect vs nice-to-have terminology needs final agreement |
| Accepted import formats | Excel is current source; additional formats are not finalized |
| Full schedule rebuild/version UI | Core logic describes it; may be implemented after MVP |
| WAYF/external VIA identity integration | Mentioned as a future question, not a current decision |

---

# 28. Minimum Version 1 endpoint checklist

The following endpoints are the minimum recommended baseline for frontend/backend integration.

| Method | Endpoint | Purpose |
|---|---|---|
| `POST` | `/api/v1/auth/login` | Login |
| `GET` | `/api/v1/auth/me` | Restore logged-in user |
| `POST` | `/api/v1/imports` | Import planning data |
| `GET` | `/api/v1/classes` | List classes |
| `GET` | `/api/v1/students` | List students |
| `GET` | `/api/v1/exams` | List exams |
| `GET` | `/api/v1/lecturers` | List lecturers/examiners |
| `GET` | `/api/v1/exams/{examId}/students/{studentId}/eligibility` | Check exam-attempt eligibility |
| `PUT` | `/api/v1/exams/{examId}/students/{studentId}` | Assign student / extra time |
| `PUT` | `/api/v1/exams/{examId}/examiners/{lecturerId}` | Assign examiner |
| `GET` | `/api/v1/lecturers/{lecturerId}/constraints` | Read constraints |
| `POST` | `/api/v1/lecturers/{lecturerId}/constraints` | Add constraint |
| `POST` | `/api/v1/schedules` | Create/generate schedule |
| `GET` | `/api/v1/schedules/current` | Open current schedule |
| `GET` | `/api/v1/schedules/{scheduleId}` | Read schedule |
| `GET` | `/api/v1/schedules/{scheduleId}/conflicts` | Scan for conflicts |
| `PATCH` | `/api/v1/schedules/{scheduleId}/sessions/{sessionId}` | Reschedule exam |
| `POST` | `/api/v1/schedules/{scheduleId}/approve` | Approve schedule |
| `GET` | `/api/v1/schedules/{scheduleId}/export?format=xlsx` | Export anonymized schedule |

The lock, rebuild, and version-history endpoints are recommended but may be implemented after the first integration milestone.

---

# 29. Contract change rules

To prevent frontend/backend mismatches:

1. Update this file before or together with a contract-changing code change.
2. Do not rename JSON fields without coordinating with both frontend and backend developers.
3. Do not change endpoint URLs or HTTP methods silently.
4. Add new optional response fields freely when they do not break existing clients.
5. A breaking API change should be explicitly discussed and may require a new API version.
6. Prefer machine-readable error codes over frontend parsing of error messages.
7. Pull requests that change the API should identify the affected endpoint(s) in the PR description.

---

# 30. Example end-to-end flow

A typical administrator workflow using this contract is:

```text
1. POST /auth/login
2. POST /imports
3. GET /classes, /students, /exams, /lecturers
4. POST /schedules
5. GET /schedules/{id}/conflicts
6. PATCH /schedules/{id}/sessions/{sessionId} when an exam must move
7. PUT /exams/{examId}/students/{studentId} when assigning a student
8. PUT /exams/{examId}/examiners/{lecturerId} when assigning an examiner
9. GET /schedules/{id}/conflicts again
10. POST /schedules/{id}/approve
11. GET /schedules/{id}/export?format=xlsx
```

Automatic email notifications are handled by the backend when the configured schedule-update events occur.

---

## End of API Contract Version 1.0