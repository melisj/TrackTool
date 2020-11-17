/// <summary>
/// This class's function is only to acitivate the network setup of the nodes and generate the mesh.
/// This will be called from thechanges in the editors and from changes to the layout of the nodes
/// </summary>
public class RealtimeManager
{
    public void Execute()
    {
        if (DataTools.NodeSetting.renderRealtime)
            TrackManager.nodeManager.SetupNetwork(true, true);
        if (DataTools.MeshSetting.renderRealtime)
            TrackManager.generator.StartGenerating();
    }
}
