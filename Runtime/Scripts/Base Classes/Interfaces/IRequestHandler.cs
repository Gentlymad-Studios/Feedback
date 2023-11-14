
namespace Feedback {
    public interface IRequestHandler {
        public void GetData(bool force);
        public void PostNewData(RequestData data);
    }
}