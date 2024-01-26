
namespace Feedback {
    public interface IRequestHandler {
        public void GetData(bool force);
        public bool PostNewData(RequestData data);
    }
}