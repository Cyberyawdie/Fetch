
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Runtime.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fetch API", Version = "v1" });
    c.SchemaFilter<MySwaggerSchemaFilter>();
});
var app = builder.Build();

// Configure the app
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fetch API V1");
    });

    // Redirect root URL to Swagger UI
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger/index.html");
            return;
        }

        await next();
    });
}


app.MapGet("/", () => "Fetch Rewards Challenge");



app.MapPost("/rewards/add-points", (Reward reward) =>
    Transaction.PostPoints(reward));

app.MapPost("/rewards/spend-points", (Spent spent) =>
   Transaction.Spend(spent));

app.MapGet("/rewards/points-balance", () =>
    Transaction.Balance());







app.Run();

public class MySwaggerSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null)
        {
            return;
        }

        var ignoreDataMemberProperties = context.Type.GetProperties()
            .Where(t => t.GetCustomAttribute<IgnoreDataMemberAttribute>() != null);

        foreach (var ignoreDataMemberProperty in ignoreDataMemberProperties)
        {
            var propertyToHide = schema.Properties.Keys
                .SingleOrDefault(x => x.ToLower() == ignoreDataMemberProperty.Name.ToLower());

            if (propertyToHide != null)
            {
                schema.Properties.Remove(propertyToHide);
            }
        }
    }
}

public class Reward
{

    public string Payer { get; set; }
    public int Points { get; set; }
    // Exclude Timestamp property from Swagger documentation
    [IgnoreDataMember]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public static Dictionary<string, Reward> RewardDict = new Dictionary<string, Reward>();



}


public class SpentOutput
{
    public string Payer { get; set; }
    public int Points { get; set; }


}

public class Spent
{
    public int Points { get; set; }


}







class Transaction
{
    //Method to add points and subtract from oldest points if a negetive point value is added
    public static List<Reward> PostPoints(Reward reward)
    {

        int subPoints = reward.Points;
        reward.Payer = reward.Payer.ToUpper();
      
        if (subPoints < 0)
        {
            var error = new List<Reward>();

            Reward reward1 = new Reward();

            reward1.Payer = "Invalid Amount";
            reward1.Points = subPoints;
            error.Add(reward1);



            return error;
        }

        if (Reward.RewardDict.ContainsKey(reward.Payer))
        {
            var rewardVal = Reward.RewardDict[reward.Payer];
            rewardVal.Points += reward.Points;
            rewardVal.Timestamp = reward.Timestamp > rewardVal.Timestamp ? reward.Timestamp : rewardVal.Timestamp;
        }
        else
        {
            Reward.RewardDict.Add(reward.Payer, reward);
        }

        

        var newQuery = Reward.RewardDict.Values.ToList();
       

        return newQuery;

    }


    //Method to calculate points spent

    public static List<SpentOutput> Spend(Spent points)
    {
        
        int point = points.Points;
        var totalCurrentPoints = Reward.RewardDict.Values.ToList().Sum(x => x.Points);

        int pointsSpent = totalCurrentPoints - point > -1 ? point : 0;

        List<Reward> rewards = Reward.RewardDict.Values.OrderBy(x => x.Timestamp).ToList();
        List<SpentOutput> result = new List<SpentOutput>();
        if (pointsSpent != 0)
        {
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
                if (currentPoints != 0 && pointsSpent! >= 0)
                {
                    pointsSpent = pointsSpent - currentPoints;

                    if (pointsSpent >= 0)
                    {
                        pointsDifference = -pointsHolder;

                        rewardSpent.Points = pointsDifference;
                        result.Add(rewardSpent);
                        reward.Points = 0;
                        if (reward.Points == 0)
                            Reward.RewardDict.Remove(reward.Payer);
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
                       
                    }

                }

            }
        }
        else
        {
            List<SpentOutput> spentError = new List<SpentOutput>();
            SpentOutput tempReward = new SpentOutput();

            tempReward.Payer = "Sorry you do not have sufficient points for this transaction";
            tempReward.Points = totalCurrentPoints;
            spentError.Add(tempReward);
            return spentError;
        }

        return result;

    }



    //Method to get currrent balance
    public static string Balance()
    {


        var rewards = Reward.RewardDict.Values.ToList();


        var dict = new Dictionary<string, int>();
        var total = 0;
        if (rewards.Count > 0)
        {
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

            total = dict.Values.Sum(x => x);
            dict.Add("TOTAL POINTS", total);

        }
        else
        {
            dict.Add("TOTAL POINTS", total);

        }

        var output = JsonConvert.SerializeObject(dict);


        return output;

    }

}






