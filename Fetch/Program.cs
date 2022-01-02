
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<FetchDb>(opt => opt.UseInMemoryDatabase("RewardsPoints"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();



app.MapGet("/", () => "Fetch Rewards Challenge");



app.MapGet("/rewards/balance", (FetchDb db) =>
    Transaction.Balance(db));


app.MapPost("/rewards/points", (FetchDb db, Reward reward) =>
    Transaction.PostPoints(db, reward));

app.MapPost("/rewards/spend", (FetchDb db, Spent p) =>
   Transaction.Spend(db, p));






app.Run();

public class Reward
{

    public int Id { get; set; }
    public string? Payer { get; set; }
    public int Points { get; set; }

    public DateTime Timestamp { get; set; }






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







class Transaction
{
    //Method to add points and subtract from oldest points if a negetive point value is added
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



                firstReward.Id = item.Id;
                firstReward.Payer = item.Payer;
                firstReward.Points = item.Points;
                firstReward.Timestamp = item.Timestamp;

                int currentPoints = item.Points;
                int hold = 0;

        // If statement to check if points added is negative and subtract that value from positive points until value is 0.
                if (currentPoints != 0 && subPoints! <= 0) 
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
        if (subPoints < 0)
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

        //Database call to show values recently added. 
        var newQuery = query.ToList();
        foreach (var item in newQuery)
        {
            if (item.Points == 0)
            {
                db.Remove(item);
                db.SaveChanges();
            }
        }

        return newQuery;

    }


    //Method to calculate points spent

    public static List<SpentOutput> Spend(FetchDb db, Spent points)
    {
        var query = (from item in db.Rewards
                     where item.Id > 0 && item.Points != 0
                     orderby item.Timestamp ascending
                     select item);


        int point = points.Points;
        int pointsSpent = point;

        List<Reward> rewards = query.ToList();
        List<SpentOutput> result = new List<SpentOutput>();
        foreach (var reward in rewards)
        {
            SpentOutput rewardSpent = new SpentOutput();

            rewardSpent.Payer = reward.Payer;
            rewardSpent.Points = reward.Points;


            int pointsHolder = reward.Points;
            int pointsDifference = 0;
            int currentPoints = reward.Points;

            //This statement block subtract the points being spent from the current points available until the points spent is 0.
            //the return value is the amount subtracted from each reward.
            if (currentPoints != 0 && pointsSpent !>= 0)
            {
                pointsSpent = pointsSpent - currentPoints;

                if (pointsSpent >= 0)
                {
                    pointsDifference = -pointsHolder;

                    rewardSpent.Points = pointsDifference;
                    result.Add(rewardSpent);
                    reward.Points = 0;
                    db.Attach(reward);
                }
                else
                {
                    if (pointsSpent > 0)
                    {
                        break;
                    }

                    pointsDifference = reward.Points - Math.Abs(pointsSpent);
                    rewardSpent.Points = -Math.Abs(pointsDifference);
                    if (rewardSpent.Points == 0)
                    {

                        break;
                    }
                    result.Add(rewardSpent);
                    reward.Points = Math.Abs(pointsSpent);
                    db.Attach(reward);

                }

            }

        }

        if (pointsSpent > 0)
        {
            List<SpentOutput> spentError = new List<SpentOutput>();
            SpentOutput tempReward = new SpentOutput();

            tempReward.Payer = "Sorry you need more points";
            tempReward.Points = 0;
            spentError.Add(tempReward);
            return spentError;
        }

        db.SaveChanges();
        return result;

    }



    //Method to get currrent balance
    public static string Balance(FetchDb db)
    {
        var query = (from item in db.Rewards
                     where item.Points >= 0
                     orderby item.Timestamp ascending
                     select item);


        List<Reward> rewards = query.ToList();


        var dict = new Dictionary<string, int>();

        foreach (var balance in rewards)
        {

            if (dict.ContainsKey(balance.Payer))
            {
                dict[balance.Payer] += balance.Points;
            }
            else
            {
                dict.Add(balance.Payer, balance.Points);
            }

        }

        var output = JsonConvert.SerializeObject(dict);


        return output;

    }

}




//In Memory Data Base
class FetchDb : DbContext
{
    public FetchDb(DbContextOptions<FetchDb> options)
        : base(options) { }

    public DbSet<Reward> Rewards => Set<Reward>();


}

