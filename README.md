# MailCheck.Scheduler
The Mail Check Scheduler Microservice is responsible for reminding other microservices to perform a task, it does this by taking in scheduled reminder requests and saves them to a database and then dispatches these reminders at the requested time
You should have a database connection established with the Scheduler DB when running any project which contains a DAO folder.