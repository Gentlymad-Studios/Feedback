public interface IRequestHandler {
    public void GetAllData();
    public void PostNewData<T1, T2>(RequestData<T1, T2> data);
}
