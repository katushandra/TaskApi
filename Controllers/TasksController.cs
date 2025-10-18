using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskApi.Data.Requests;
using TaskApi.Data.Responses;
using TaskApi.Interfaces;

namespace TaskApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : BaseApiController
    {
        private readonly ITaskService taskService;

        public TasksController(ITaskService taskService)
        {
            this.taskService = taskService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var getTaskResponse = await taskService.GetTasks(UserID);
            if (!getTaskResponse.Success)
            {
                return UnprocessableEntity(getTaskResponse);
            }
            var taskResponse = getTaskResponse.Tasks.ConvertAll(o => new TaskResponse { Id = o.Id, IsCompleted = o.IsCompleted, Name = o.Name, Ts = o.Ts });
            return Ok(getTaskResponse);
        }

        [HttpPost]
        public async Task<IActionResult> Post(TaskRequest taskRequest)
        {
            var task = new Data.Entities.Taskdb
            {
                IsCompleted = taskRequest.IsCompleted,
                Ts = DateTime.Now,
                Name = taskRequest.Name,
                UserId = UserID
            };

            var saveTaskResponse = await taskService.SaveTask(task);
            if (!saveTaskResponse.Success)
            {
                return UnprocessableEntity(saveTaskResponse);
            }
            var taskResponse = new TaskResponse { Id = saveTaskResponse.Task.Id, IsCompleted = saveTaskResponse.Task.IsCompleted, Name = saveTaskResponse.Task.Name, Ts = DateTime.UtcNow };
            return Ok(taskResponse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleteTaskResponse = await taskService.DeleteTask(id, UserID);
            if (!deleteTaskResponse.Success)
            {
                return UnprocessableEntity(deleteTaskResponse);
            }
            return Ok(deleteTaskResponse.TaskId);
        }
    }
}