using System.Net.Sockets;
using VSIXService.Helpers;

namespace VSIXService;

internal class Worker
{
    private readonly TcpClient client = null;

    private readonly NetworkStream stream = null;

    /// <summary>
    /// worker唯一标识
    /// </summary>
    private readonly string WrokId = Guid.NewGuid().ToString("N");

    /// <summary>
    /// 上下文对象
    /// </summary>
    public WorkContent Content { get; private set; } = null;

    /// <summary>
    /// 每个请求连接分配到一个Worker对象,通信在单独线程中进行,不占用主线程.每次通信后,关闭这个连接.
    /// </summary>
    /// <param name="client"></param>
    public Worker(TcpClient client)
    {
        try
        {
            this.client = client;
            this.stream = client.GetStream();
            this.Content = new();
            LogHelp.SrvLog($"[{this.WrokId} Start]请求开始处理,进入通信线程(1,2,3,4)...");
            this.Run();
        }
        catch (Exception e)
        {
            LogHelp.SrvLog($"[{this.WrokId} Faild]请求处理异常: {e.Message}");
        }
    }

    /// <summary>
    /// 处理请求内容
    /// </summary>
    private void Run()
    {
        // 读取消息线程
        Task.Run(() =>
        {
            try
            {
                ParseInput();
                LogHelp.SrvLog($"[{this.WrokId} Parse]1.命令已解析... id: [{Content.CmdId}] para: [{Content.ParaString}]");
                Commander.Execute(Content);
                LogHelp.SrvLog($"[{this.WrokId} Exec]2.命令已执行...[{Content.ResultCode}]");
                ReturnOutput();
                LogHelp.SrvLog($"[{this.WrokId} Return]3.结果已返回.");
            }
            catch (Exception e)
            {
                LogHelp.SrvLog($"[{this.WrokId} Faild]消息处理异常: {e.Message}");
            }
            finally
            {
                // close();
                stream.Close();
                client.Close();
                LogHelp.SrvLog($"[{this.WrokId} END]4.结束处理,连接关闭.");
            }
        });
    }

    /// <summary>
    /// 返回数据
    /// 协议:第1个字节是命令是否成功,后面是数据
    /// </summary>
    /// <returns></returns>
    private void ReturnOutput()
    {
        List<byte> data = new();
        data.Add(Content.ResultCode);
        data.AddRange(Content.Result.ToBytesUTF8());
        stream.Write(data.ToArray(), 0, data.Count);
    }

    /// <summary>
    /// 协议:第1个字节是命令id,后面是参数
    /// </summary>
    /// <param name="content"></param>
    private void ParseInput()
    {
        byte[] buffer = new byte[1024];
        int lastIndex = stream.Read(buffer, 0, buffer.Length);
        if (lastIndex == 0)
            throw new VSIXServiceException("No any params,the CmdId is must!");
        Content.CmdId = buffer[0];
        Content.ParaString = buffer[1..lastIndex].ToStringUTF8();
    }
}