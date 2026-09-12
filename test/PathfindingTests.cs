namespace RunecraftHelper.Tests;
using System.Numerics;
using Xunit;
public sealed class PathfindingTests
{
    [Fact]
    public void BoundsAndCancellation()
    {
        var data = Enumerable.Repeat((byte)0x11, 128).ToArray();
        Assert.False(LineWalker.IsWalkable(data, 8, -1, 0));
        Assert.False(LineWalker.IsWalkable(data, 8, 16, 0));
        using var stop = new CancellationTokenSource();stop.Cancel();
        Assert.Throws<OperationCanceledException>(()=>WalkablePathfinder.FindPath(data,8,Vector2.Zero,Vector2.One,cancellationToken:stop.Token));
        Assert.Throws<OperationCanceledException>(()=>WalkablePathfinder.FindPathCost(data,8,Vector2.Zero,Vector2.One,cancellationToken:stop.Token));
    }
    [Fact]
    public void AStarAgreesWithDijkstraOnRandomMaps()
    {
        var random=new Random(71);
        for(int map=0;map<100;map++)
        {
            const int w=16,bpr=8;
            var data=new byte[w*bpr];
            for(int i=0;i<data.Length;i++)data[i]=(byte)((random.Next(4)==0?0:1)|(random.Next(4)==0?0:16));
            data[0]|=1;data[^1]|=16;
            var distances=Enumerable.Repeat(float.PositiveInfinity,w*w).ToArray();distances[0]=0;
            var queue=new PriorityQueue<int,float>();queue.Enqueue(0,0);
            while(queue.TryDequeue(out var at,out var cost))
            {
                if(cost!=distances[at])continue;
                int x=at%w,y=at/w;
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)
                {
                    if(dx==0&&dy==0)continue;int nx=x+dx,ny=y+dy;
                    if(!LineWalker.IsWalkable(data,bpr,nx,ny))continue;
                    if(dx!=0&&dy!=0&&(!LineWalker.IsWalkable(data,bpr,x+dx,y)||!LineWalker.IsWalkable(data,bpr,x,y+dy)))continue;
                    float next=cost+(dx!=0&&dy!=0?1.41421356f:1f);int index=ny*w+nx;
                    if(next<distances[index]){distances[index]=next;queue.Enqueue(index,next);}
                }
            }
            var actual=WalkablePathfinder.FindPathCost(data,bpr,Vector2.Zero,new(w-1,w-1));
            if(float.IsPositiveInfinity(distances[^1]))Assert.Equal(-1f,actual);
            else Assert.InRange(Math.Abs(actual-distances[^1]),0,0.001f);
        }
    }
}
