public interface IRequestHandler {
    public void GetData(bool force);
    public void PostNewData<T1, T2>(RequestData<T1, T2> data);
}
