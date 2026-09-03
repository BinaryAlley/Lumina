# Lumina API

- [Lumina API](#lumina-api)
  - [Scheduled Jobs](#scheduled-jobs)
    - [Add Scheduled Job](#add-scheduled-job)
      - [Add Scheduled Job Request](#add-scheduled-job-request)
      - [Add Scheduled Job Response](#add-scheduled-job-response)
    - [Get Scheduled Jobs](#get-scheduled-jobs)
      - [Get Scheduled Jobs Request](#get-scheduled-jobs-request)
      - [Get Scheduled Jobs Response](#get-scheduled-jobs-response)
    - [Get Scheduled Job History](#get-scheduled-job-history)
      - [Get Scheduled Job History Request](#get-scheduled-job-history-request)
      - [Get Scheduled Job History Response](#get-scheduled-job-history-response)
    - [Remove Scheduled Job](#remove-scheduled-job)
      - [Remove Scheduled Job Request](#remove-scheduled-job-request)
      - [Remove Scheduled Job Response](#remove-scheduled-job-response)
    - [Start Scheduled Job](#start-scheduled-job)
      - [Start Scheduled Job Request](#start-scheduled-job-request)
      - [Start Scheduled Job Response](#start-scheduled-job-response)
    - [Stop Scheduled Job](#stop-scheduled-job)
      - [Stop Scheduled Job Request](#stop-scheduled-job-request)
      - [Stop Scheduled Job Response](#stop-scheduled-job-response)
    - [Fire Scheduled Job](#fire-scheduled-job)
      - [Fire Scheduled Job Request](#fire-scheduled-job-request)
      - [Fire Scheduled Job Response](#fire-scheduled-job-response)

## Scheduled Jobs

### Add Scheduled Job

#### Add Scheduled Job Request

```js
POST api/v1/scheduled-jobs
```

```json
{
  "name": "Rescan media libraries",
  "taskType": "ScanMediaLibraries",
  "scheduleType": "DailyAtHourAndMinute",
  "intervalMinutes": null,
  "hour": 3,
  "minute": 0
}
```

#### Add Scheduled Job Response

```js
200 Ok
```

```json
{
  "id": "d0b8e2a4-3f9c-4b6d-8e7a-1c5f2a9b4e6d",
  "name": "Rescan media libraries",
  "taskType": "ScanMediaLibraries",
  "scheduleType": "DailyAtHourAndMinute",
  "intervalMinutes": null,
  "hour": 3,
  "minute": 0,
  "status": "Added",
  "lastStartedOnUtc": null,
  "lastCompletedOnUtc": null
}
```

### Get Scheduled Jobs

#### Get Scheduled Jobs Request

```js
GET api/v1/scheduled-jobs
```

#### Get Scheduled Jobs Response

```js
200 Ok
```

```json
[
  {
    "id": "d0b8e2a4-3f9c-4b6d-8e7a-1c5f2a9b4e6d",
    "name": "Rescan media libraries",
    "taskType": "ScanMediaLibraries",
    "scheduleType": "DailyAtHourAndMinute",
    "intervalMinutes": null,
    "hour": 3,
    "minute": 0,
    "status": "Active",
    "lastStartedOnUtc": "2026-09-02T03:00:00.0000000Z",
    "lastCompletedOnUtc": "2026-09-02T03:00:42.0000000Z"
  },
  {
    "id": "8c3f7b2e-1a4d-4f9c-b8e6-7d2a5f1c9e4b",
    "name": "Clean temporary files",
    "taskType": "CleanTemporaryFiles",
    "scheduleType": "WithIntervalInMinutes",
    "intervalMinutes": 60,
    "hour": null,
    "minute": null,
    "status": "Added",
    "lastStartedOnUtc": null,
    "lastCompletedOnUtc": null
  }
]
```

### Get Scheduled Job History

#### Get Scheduled Job History Request

```js
GET api/v1/scheduled-jobs/history?from=2026-09-02T00:00:00.0000000Z&to=2026-09-03T00:00:00.0000000Z
```

#### Get Scheduled Job History Response

```js
200 Ok
```

```json
[
  {
    "id": "e5a1c9d4-6b7f-4a2e-9d3c-8f4b1a6c2d5e",
    "scheduledJobId": "d0b8e2a4-3f9c-4b6d-8e7a-1c5f2a9b4e6d",
    "taskType": "ScanMediaLibraries",
    "isCycleRun": true,
    "startedOnUtc": "2026-09-02T03:00:00.0000000Z",
    "completedOnUtc": "2026-09-02T03:00:42.0000000Z"
  },
  {
    "id": "7f2d4e8a-3c5b-4f1a-9b6e-2a8c4d7e1f3b",
    "scheduledJobId": "d0b8e2a4-3f9c-4b6d-8e7a-1c5f2a9b4e6d",
    "taskType": "ScanMediaLibraries",
    "isCycleRun": false,
    "startedOnUtc": "2026-09-02T08:15:00.0000000Z",
    "completedOnUtc": "2026-09-02T08:15:55.0000000Z"
  }
]
```

### Remove Scheduled Job

#### Remove Scheduled Job Request

```js
DELETE api/v1/scheduled-jobs/{scheduledJobId}
```

#### Remove Scheduled Job Response

```js
200 Ok
```

### Start Scheduled Job

#### Start Scheduled Job Request

```js
PUT api/v1/scheduled-jobs/{scheduledJobId}/start
```

#### Start Scheduled Job Response

```js
200 Ok
```

```json
{
  "id": "d0b8e2a4-3f9c-4b6d-8e7a-1c5f2a9b4e6d",
  "name": "Rescan media libraries",
  "taskType": "ScanMediaLibraries",
  "scheduleType": "DailyAtHourAndMinute",
  "intervalMinutes": null,
  "hour": 3,
  "minute": 0,
  "status": "Active",
  "lastStartedOnUtc": null,
  "lastCompletedOnUtc": null
}
```

### Stop Scheduled Job

#### Stop Scheduled Job Request

```js
PUT api/v1/scheduled-jobs/{scheduledJobId}/stop
```

#### Stop Scheduled Job Response

```js
200 Ok
```

```json
{
  "id": "d0b8e2a4-3f9c-4b6d-8e7a-1c5f2a9b4e6d",
  "name": "Rescan media libraries",
  "taskType": "ScanMediaLibraries",
  "scheduleType": "DailyAtHourAndMinute",
  "intervalMinutes": null,
  "hour": 3,
  "minute": 0,
  "status": "Added",
  "lastStartedOnUtc": "2026-09-02T03:00:00.0000000Z",
  "lastCompletedOnUtc": "2026-09-02T03:00:42.0000000Z"
}
```

### Fire Scheduled Job

#### Fire Scheduled Job Request

```js
PUT api/v1/scheduled-jobs/{scheduledJobId}/fire
```

#### Fire Scheduled Job Response

```js
200 Ok
```

```json
{
  "id": "d0b8e2a4-3f9c-4b6d-8e7a-1c5f2a9b4e6d",
  "name": "Rescan media libraries",
  "taskType": "ScanMediaLibraries",
  "scheduleType": "DailyAtHourAndMinute",
  "intervalMinutes": null,
  "hour": 3,
  "minute": 0,
  "status": "Active",
  "lastStartedOnUtc": null,
  "lastCompletedOnUtc": null
}
```
