using TaskApi.Data.Responses;

namespace TaskApi.Interfaces
{
    public interface ITaskService
    {
        Task<GetTasksResponse> GetTasks(int userId);
        Task<SaveTaskResponse> SaveTask(Data.Entities.Task task);
        Task<DeleteTaskResponse> DeleteTask(int taskId, int userId);
    }
}
