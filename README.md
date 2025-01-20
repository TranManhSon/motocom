Build 
```
docker build -t motocom-api
docker run -d -p 8080:80 --name motocom-container motocom-api
```

Motocom
```
POST /api/robot/connect 
POST /api/robot/servo/on
POST /api/robot/servo/off
GET /api/robot/jobs
DELETE /api/robot/jobs/{jobName}

/api/robot/status/update-detailed
/api/robot/status/update
/api/robot/alarm/reset
/api/robot/hold/on
/api/robot/hold/off
/api/robot/start

POST /api/robot/variables/simple/read
#GET /api/robot/variables/position/{index}
#POST /api/robot/variables/position/write
POST /api/robot/variables/simple/write

GET /api/robot/file/read/{fileTitle}
GET /api/robot/file/read-temp/{fileTitle}
POST /api/robot/io/write-single
GET /api/robot/io/read-groups
POST /api/robot/io/write-groups
GET /api/robot/io/read-single/{address}


POST /api/robot/job/star
GET /api/robot/job/current
POST /api/robot/job/set-current
GET /api/robot/task/{taskId}/current-line
```