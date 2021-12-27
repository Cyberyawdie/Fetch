
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<FetchDb>(opt => opt.UseInMemoryDatabase("RewardsPoints"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "Fetch Rewards Challenge");

app.MapGet("/rewards", async (FetchDb db) =>
    await db.Rewards.ToListAsync());

app.MapGet("/rewards/balance", (FetchDb db) =>
    Transaction.Balance(db));


app.MapPost("/rewards/points", (FetchDb db, Reward reward) =>
    Transaction.PostPoints(db,reward));

app.MapPost("/rewards/spend/", (FetchDb db, Spent p) =>
   Transaction.Spend(db, p));

app.MapGet("/rewards/{id}", async (int id, FetchDb db) =>
    await db.Rewards.FindAsync(id)
        is Reward reward
            ? Results.Ok(reward)
            : Results.NotFound());


//app.MapPut("/rewards/{id}", async (int id, Reward inputReward, FetchDb db) =>
//{
//    var reward = await db.Rewards.FindAsync(id);

//    if (reward is null) return Results.NotFound();

//    reward.Payer = inputReward.Payer;
//    reward.Points = inputReward.Points;

//    await db.SaveChangesAsync();

//    return Results.NoContent();
//});

app.MapDelete("/rewards/{id}", async (int id, FetchDb db) =>
{
    if (await db.Rewards.FindAsync(id) is Reward reward)
    {
        db.Rewards.Remove(reward);
        await db.SaveChangesAsync();
        return Results.Ok(reward);
    }

    return Results.NotFound();
});

app.Run();

public class Reward
{
    
    public int Id { get; set; }
    public string? Payer { get; set; }
    public int Points { get; set; }

   public DateTime Timestamp { get; set; } = DateTime.UtcNow;
     
      
  

    
   
}


public class SpentOutput
{
    public string? Payer { get; set; }
    public int Points { get; set; }

    
}

public class Spent
{
    public int Points { get; set; }

    
}

public class BalanceOutput
{
    
    public string? Payer { get; set; }
    public int Points { get; set; }

    
}






class Transaction
{
    public static List<Reward> PostPoints(FetchDb db, Reward reward)
    {
          var query = from rewards in db.Rewards
                      where rewards.Id > 0 && rewards.Points != 0
                      orderby rewards.Timestamp ascending
                      
                      select rewards;
         
        List<Reward> rewardList = query.ToList();
        
     int posPoints = reward.Points;
            int subPoints = posPoints;
        if (subPoints < 0 && rewardList != null)
        {
            List<Reward> result = new List<Reward>();   
          
            foreach (var item in rewardList)
            {
                Reward firstReward = item;

                //update oldest reward points
                
                firstReward.Id = item.Id;
                firstReward.Payer = item.Payer;
                firstReward.Points = item.Points;
                firstReward.Timestamp = item.Timestamp;

                 int currentPoints = item.Points;
                int hold = 0;

                if (currentPoints != 0 && subPoints!<=0)
                {
                    subPoints = currentPoints - Math.Abs(subPoints);

                    if (subPoints >= 0)
                    {
                        hold = subPoints;
                       
                        firstReward.Points = hold;

                        db.Attach(firstReward);
                       

                    }
                    else
                    {
                        hold = 0;
                        firstReward.Points = hold;
                        db.Remove(firstReward);
                    }
                }
             
                

            }
           

           
            
        }
        else
        {

            db.Rewards.Add(reward);
            

        }
        if(subPoints < 0)
        {
            var error = new List<Reward>();

            Reward reward1 = new Reward();

            reward1.Payer = "Invalid Amount";
            reward1.Points = subPoints;
            error.Add(reward1);

            

            return error;
        }
        else
        {
            db.SaveChanges();
        }

        
        var newQuery = query.ToList();

        return newQuery;



     }

    public static List<SpentOutput> Spend(FetchDb db, Spent points)
    {
        var query = (from item in db.Rewards
                     where item.Id > 0 && item.Points != 0
                     orderby item.Timestamp ascending
                     select item);
                    

        int point = points.Points;
        int sub = point;
       
        List<Reward> rewards = query.ToList();
        List<SpentOutput> result = new List<SpentOutput>();
        foreach (var item in rewards)
        {
            SpentOutput tempReward = new SpentOutput();
            
            tempReward.Payer = item.Payer;
            tempReward.Points = item.Points;
            

            int holder = item.Points;
            int difference =0;
            int temp = item.Points;

            if (temp != 0 && sub !>= 0)
            {
                sub = sub - temp;
               
                if (sub >= 0)
                {
                difference = -holder;

                    tempReward.Points = difference;
                    result.Add(tempReward);
                    item.Points = 0;
                    db.Attach(item);
                }
                else
                {
                    if (sub > 0)
                    {
                        break;
                    }

                    difference = item.Points - Math.Abs(sub);
                    tempReward.Points = -Math.Abs(difference);
                    if (tempReward.Points == 0)
                    {
                        
                        break;
                    }
                    result.Add(tempReward);
                    item.Points = Math.Abs(sub);
                    db.Attach(item);
                    
                }
                



            }
            

        }
        
                    if (sub > 0)
            {
            List<SpentOutput> tempSpent = new List<SpentOutput>();
            SpentOutput tempReward = new SpentOutput();

            tempReward.Payer = "Sorry you need more points";
            tempReward.Points = 0;
            tempSpent.Add(tempReward);
            return tempSpent;
            }
      
db.SaveChanges();
        return result;

    }

    public static List<BalanceOutput> Balance(FetchDb db)
    {
        var query = (from item in db.Rewards
                     where item.Id > 0 && item.Points >= 0
                     orderby item.Timestamp ascending
                     select item);
               

        List<Reward> rewards = query.ToList();
        List<BalanceOutput> result = new List<BalanceOutput>();

        foreach (var item in rewards)
        {
            BalanceOutput balanceOutput = new BalanceOutput();

            balanceOutput.Payer = item.Payer;
            balanceOutput.Points = item.Points;

           
            result.Add(balanceOutput);
            

        }







        return result;
    }


}

 

class FetchDb : DbContext
{
    public FetchDb(DbContextOptions<FetchDb> options)
        : base(options) { }

    public DbSet<Reward> Rewards => Set<Reward>();
    
    
}

