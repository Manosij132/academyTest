using Academy.Core.Abstraction.Services;
using Academy.Domain.Entities;
using Academy.Shared.DTO;
using Arch.EntityFrameworkCore;
using Arch.EntityFrameworkCore.UnitOfWork;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.ML;
using System.Data;

namespace Academy.Core.Services
{
    //AGK API Migration
    public class SlotRequirementService : ISlotRequirementService
    {
        private readonly IRepository<PanelSlotsRequirement> _slotsRequirementRepository;
        private readonly IRepository<CommunitySelectionRatio> _communitySelectionRatio;
        private readonly IRepository<Domain.Entities.InterviewData> _interviewData;
        private readonly IRepository<InterviewPanelDetails> _InterviewPanelRepository;
        private readonly IRepository<PanelSlots> _panelSlots;
        private readonly IRepository<SeniorityMaster> _seniority;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IAuthenticatedUserService _authenticatedUserService;
        public SlotRequirementService(IMapper mapper, IUnitOfWork unitOfWork, IConfiguration configuration, IAuthenticatedUserService authenticatedUserService)
        {
            _slotsRequirementRepository = unitOfWork.GetRepository<PanelSlotsRequirement>();
            _communitySelectionRatio = unitOfWork.GetRepository<CommunitySelectionRatio>();
            _interviewData = unitOfWork.GetRepository<Domain.Entities.InterviewData>();
            _InterviewPanelRepository = unitOfWork.GetRepository<InterviewPanelDetails>();
            _seniority = unitOfWork.GetRepository<SeniorityMaster>();
            _panelSlots = unitOfWork.GetRepository<PanelSlots>();
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _authenticatedUserService = authenticatedUserService;
        }
        public async Task<List<SlotRequirementModel>> GetAllSlotManagement(string TDC, int communityID, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                List<PanelSlotsRequirement> slotReqData = new();
                IQueryable<PanelSlotsRequirement> query = _slotsRequirementRepository.GetAll();
                if (string.IsNullOrEmpty(TDC) || communityID == 0)
                {
                    return null;
                }
                else if (query != null)
                {
                    if (!string.IsNullOrEmpty(TDC))
                        query = query.Where(t => t.TDC.Equals(TDC));
                    if (communityID != 0)
                        query = query.Where(t => t.CommunityId.Equals(communityID));
                    if (startDate.HasValue && endDate.HasValue)
                        query = query.Where(t => t.StartDate >= startDate && t.EndDate <= endDate.Value.AddDays(1));
                                        
                    return _mapper.Map<List<PanelSlotsRequirement>, List<SlotRequirementModel>>(query?.ToList());

                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<CommunitySelectionRatioModel> GetCommunitySelectionRatio(string TDC = null, int communityID = 0, DateTime? startDate = null, DateTime? endDate = null)
        {
            CommunitySelectionRatio? communitySelectionRatio = null;
            Task<CommunitySelectionRatioModel> communitySelectionRatioTask = null;
            try
            {
                communitySelectionRatio = _communitySelectionRatio.GetAll().FirstOrDefault(x => x.TDC == TDC && x.CommunityId == communityID);

                if (startDate.HasValue && endDate.HasValue)
                {
                    communitySelectionRatio = _communitySelectionRatio.GetAll().FirstOrDefault(x => x.TDC == TDC && x.CommunityId == communityID && x.StartDate >= startDate && x.EndDate <= endDate);
                }
                else
                {
                    communitySelectionRatio = _communitySelectionRatio.GetAll().FirstOrDefault(x => x.TDC == TDC && x.CommunityId == communityID);
                }

                if (communitySelectionRatio != null)
                {
                    return new CommunitySelectionRatioModel()
                    {
                        L1SelectionRatio = communitySelectionRatio?.L1SelectionRatio,
                        GKSelectionRatio = communitySelectionRatio?.GKSelectionRatio
                    };
                }
                else
                {
                    return GetPredicatedRatio(TDC, communityID, startDate, endDate).Result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<CommunitySelectionRatioModel> GetPredicatedRatio(string? TDC = null, int communityId = 0, DateTime? startDate = null, DateTime? endDate = null)
        {
            CommunitySelectionRatioModel predictedSelectionRatio = new();
            try
            {
                predictedSelectionRatio = PredicatedRatio();

                var selectionRatio = _communitySelectionRatio.GetAll().ToList();
                var result = selectionRatio.FirstOrDefault(x => x.TDC == TDC && x.CommunityId == communityId && x.StartDate >= startDate && x.EndDate <= endDate);
                if (result != null)
                {
                    result.L1SelectionRatio = predictedSelectionRatio.L1SelectionRatio;
                    result.GKSelectionRatio = predictedSelectionRatio.GKSelectionRatio;
                    result.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                    result.UpdatedOn = DateTime.UtcNow;
                    _communitySelectionRatio.Update(result);
                }
                else
                {
                    CommunitySelectionRatio communitySelectionRatio = new()
                    {
                        TDC = TDC,
                        CommunityId = communityId,
                        StartDate = startDate,
                        EndDate = endDate,
                        L1SelectionRatio = predictedSelectionRatio.L1SelectionRatio,
                        GKSelectionRatio = predictedSelectionRatio.GKSelectionRatio,
                        CreatedBy = _authenticatedUserService.AuthUser.Id,
                        CreatedOn = DateTime.UtcNow
                    };
                    _communitySelectionRatio.Insert(communitySelectionRatio);
                }
                _unitOfWork.SaveChanges();

                return await GetCommunitySelectionRatio(TDC, communityId, startDate, endDate);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            return predictedSelectionRatio;
        }

        private CommunitySelectionRatioModel PredicatedRatio()
        {
            CommunitySelectionRatioModel predictedSelectionRatioModel = new();
            var interviewDBDatas = FetchRecordsAsync();

            // Create a new ML context
            MLContext mlContext = new MLContext();

            // Load data
            //IDataView dataView = mlContext.Data.LoadFromEnumerable(interviewDBDatas);

            Academy.Shared.DTO.InterviewData eveluateData = new Academy.Shared.DTO.InterviewData()
            {
                L1Select = Convert.ToSingle(interviewDBDatas.Sum(x => x.L1Select)),
                L1Reject = Convert.ToSingle(interviewDBDatas.Sum(x => x.L1Reject)),
                GKSelect = Convert.ToSingle(interviewDBDatas.Sum(x => x.GKSelect)),
                GKReject = Convert.ToSingle(interviewDBDatas.Sum(x => x.GKReject)),
                TotalCandidates = Convert.ToSingle(interviewDBDatas.Sum(x => x.TotalCandidates)),

            };

            // Load data
            string dataPath = _configuration["ConnectionString:FIlePath"];
            IDataView dataView = mlContext.Data.LoadFromTextFile<Academy.Shared.DTO.InterviewData>(dataPath, hasHeader: true, separatorChar: ',');


            var L1Ratio = L1TrainFastTree(mlContext, dataView, eveluateData);
            var gkRatio = GKTrainFastTree(mlContext, dataView, eveluateData);
            predictedSelectionRatioModel.L1SelectionRatio = Math.Round(Convert.ToDecimal(L1Ratio), 2);
            predictedSelectionRatioModel.GKSelectionRatio = Math.Round(Convert.ToDecimal(gkRatio), 2);

            return predictedSelectionRatioModel;

        }

        public List<Academy.Shared.DTO.InterviewData> FetchRecordsAsync()
        {
            var result = _interviewData.GetAll().ToList();
            List<Academy.Shared.DTO.InterviewData> interviewDatas = new List<Academy.Shared.DTO.InterviewData>();
            foreach (var item in result)
            {
                interviewDatas.Add(new Academy.Shared.DTO.InterviewData()
                {
                    L1Select = Convert.ToSingle(item.L1Select),
                    L1Reject = Convert.ToSingle(item.L1Reject),
                    GKSelect = Convert.ToSingle(item.GKSelect),
                    GKReject = Convert.ToSingle(item.GKReject),
                    TotalCandidates = item.GrandTotal == null ? 0.0f : Convert.ToSingle(item.GrandTotal), // Convert to Single (float), handle DBNull
                });
            }

            return interviewDatas;
        }
        private float L1TrainFastTree(MLContext mlContext, IDataView dataView, Academy.Shared.DTO.InterviewData? dbData)
        {
            Console.WriteLine("L1 training...[FastForest]");
            // Split data into train and test sets
            var splitData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            var trainData = splitData.TrainSet;
            var testData = splitData.TestSet;

            // Define data preparation and model training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", nameof(Academy.Shared.DTO.InterviewData.L1Select), nameof(Academy.Shared.DTO.InterviewData.L1Reject), nameof(Academy.Shared.DTO.InterviewData.GKSelect), nameof(Academy.Shared.DTO.InterviewData.GKReject), nameof(Academy.Shared.DTO.InterviewData.TotalCandidates))
                .Append(mlContext.Regression.Trainers.FastForest(labelColumnName: nameof(Academy.Shared.DTO.InterviewData.L1CandidatesPerSelection)));

            // var pipeline = context.Transforms.Concatenate("Features", nameof(InterviewData.GKReject), nameof(InterviewData.GKSelect), nameof(InterviewData.L1Select), nameof(InterviewData.L1Reject), nameof(InterviewData.GrandTotal))
            //.Append(context.Regression.Trainers.Sdca(labelColumnName: nameof(InterviewData.InterviewsPerSelection), featureColumnName: "Features")); //1.9


            // Train the model
            var model = pipeline.Fit(trainData);

            // Evaluate the model
            var predictions = model.Transform(testData);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: nameof(Academy.Shared.DTO.InterviewData.L1CandidatesPerSelection));

            Console.WriteLine($"R^2: {metrics.RSquared}");
            Console.WriteLine($"RMSE: {metrics.RootMeanSquaredError}");

            // Save the model
            mlContext.Model.Save(model, dataView.Schema, "MLModel_L1.zip");

            //Console.WriteLine("Model training complete.");

            // Make a prediction
            return L1Predict(mlContext, model, dbData);
        }

        static float L1Predict(MLContext mlContext, ITransformer model, Academy.Shared.DTO.InterviewData interviewDB = null)
        {
            var predictionEngine = mlContext.Model.CreatePredictionEngine<Academy.Shared.DTO.InterviewData, L1InterviewPrediction>(model);

            // Example data for prediction
            var newSample = new Academy.Shared.DTO.InterviewData
            {
                L1Select = interviewDB.L1Select,
                L1Reject = interviewDB.L1Reject,
                GKSelect = interviewDB.GKSelect,
                GKReject = interviewDB.GKReject,
                TotalCandidates = interviewDB.TotalCandidates,
            };

            var prediction = predictionEngine.Predict(newSample);
            // Console.WriteLine($"Predicted L1 selection ratio: {prediction.L1CandidatesPerSelection}");

            return prediction.L1CandidatesPerSelection;

            //string query = $"UPdate [InterviewData] SET AIPredict = {prediction.Score} WHERE Id = {interviewDB.Id};";
            //await UpdateRecordsByIdAsync(query);
        }

        private float GKTrainFastTree(MLContext mlContext, IDataView dataView, Academy.Shared.DTO.InterviewData dbData)
        {
            Console.WriteLine("Method 2...[FastForest]");
            // Split data into train and test sets
            var splitData = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);
            var trainData = splitData.TrainSet;
            var testData = splitData.TestSet;

            // Define data preparation and model training pipeline
            var pipeline = mlContext.Transforms.Concatenate("Features", "L1Select", "L1Reject", "GKSelect", "GKReject", "TotalCandidates")
                .Append(mlContext.Regression.Trainers.FastForest(labelColumnName: "GKCandidatesPerSelection"));

            // Train the model
            var model = pipeline.Fit(trainData);

            // Evaluate the model
            var predictions = model.Transform(testData);
            var metrics = mlContext.Regression.Evaluate(predictions, labelColumnName: "GKCandidatesPerSelection");

            Console.WriteLine($"R^2: {metrics.RSquared}");
            Console.WriteLine($"RMSE: {metrics.RootMeanSquaredError}");

            // Save the model
            mlContext.Model.Save(model, dataView.Schema, "MLModel.zip");

            //Console.WriteLine("Model training complete.");

            // Make a prediction
            return L1Predict(mlContext, model, dbData);
        }

        public async Task<CommunitySelectionRatioModel> UpdateCommunitySelectionRatio(CommunitySelectionRatioModel communitySelectionRatioModel)
        {
            var selectionRatio = _communitySelectionRatio.GetAll().ToList();
            var result = selectionRatio.FirstOrDefault(x => x.TDC == communitySelectionRatioModel.TDC && x.CommunityId == communitySelectionRatioModel.CommunityId && x.StartDate >= communitySelectionRatioModel.StartDate && x.EndDate <= communitySelectionRatioModel.EndDate);
            if (result != null)
            {
                result.L1SelectionRatio = communitySelectionRatioModel.L1SelectionRatio;
                result.GKSelectionRatio = communitySelectionRatioModel.GKSelectionRatio;
                result.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                result.UpdatedOn = DateTime.UtcNow;
                _communitySelectionRatio.Update(result);
            }
            else
            {
                CommunitySelectionRatio communitySelectionRatio = new()
                {
                    TDC = communitySelectionRatioModel.TDC,
                    CommunityId = communitySelectionRatioModel.CommunityId,
                    StartDate = communitySelectionRatioModel.StartDate,
                    EndDate = communitySelectionRatioModel.EndDate,
                    L1SelectionRatio = communitySelectionRatioModel.L1SelectionRatio,
                    GKSelectionRatio = communitySelectionRatioModel.GKSelectionRatio,
                    CreatedBy = _authenticatedUserService.AuthUser.Id,
                    CreatedOn = DateTime.UtcNow
                };
                _communitySelectionRatio.Insert(communitySelectionRatio);
            }
            _unitOfWork.SaveChanges();
            return communitySelectionRatioModel;
        }

        public async Task<bool> UpdateSlotManagement(List<SlotRequirementModel> slotRequirementModel)
        {
            foreach (var model in slotRequirementModel)
            {
                if (model.StartDate.HasValue && model.EndDate.HasValue)
                {
                    model.StartDate = TimeZoneInfo.ConvertTimeFromUtc(model.StartDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    model.EndDate = TimeZoneInfo.ConvertTimeFromUtc(model.EndDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                }
            }
            bool response = false;
            List<PanelSlotsRequirement> panelSlotsRequirementList = CreateModel(slotRequirementModel, true);
            _slotsRequirementRepository.Update(panelSlotsRequirementList);
            _unitOfWork.SaveChanges();
            response = true;
            return response;
        }

        private List<PanelSlotsRequirement> CreateModel(List<SlotRequirementModel> slotRequirementModelList, bool updateSlot)
        {
            var interviewPanel = _InterviewPanelRepository.GetAll().ToList();
            var panelSlots = _panelSlots.GetAll().ToList();


            List<PanelSlotsRequirement> backendModels = new List<PanelSlotsRequirement>();
            foreach (var slotRequirementModel in slotRequirementModelList)
            {
                if (slotRequirementModel.PositionToBeFilled == 0 && slotRequirementModel.DropRatio == 0 && slotRequirementModel.OffersToBeRolledOut == 0)
                {
                    var deleted = _slotsRequirementRepository.GetAll().Where(x => x.Id == slotRequirementModel.Id);
                    _slotsRequirementRepository.Delete(deleted);
                    _unitOfWork.SaveChanges();
                }
                else
                {
                    int? L1SlotCounts = null;
                    int? GKSlotCounts = null;
                    int? L1PanelCount = null;
                    int? GKPanelCount = null;

                    if (slotRequirementModel.PositionToBeFilled != 0 && slotRequirementModel.DropRatio != 0 && slotRequirementModel.OffersToBeRolledOut != 0)
                    {
                        var senioity = _seniority.GetAll().FirstOrDefault(x => x.SeniorityId == slotRequirementModel.SeniorityId)?.SeniorityName;

                        var panels = interviewPanel.Where(x => x.CommunityId == slotRequirementModel.CommunityId && x.SeniorityUpTo == senioity && x.TDC == slotRequirementModel.TDC).ToList();

                        if (slotRequirementModel.L1SlotsRequired > 0)
                        {
                            var L1Panels = panelSlots.Where(x => panels.Where(x => x.Type == "L1").Select(x => x.Id).Contains(x.PanelId) && x.SlotDate >= slotRequirementModel.StartDate && x.SlotDate <= slotRequirementModel.EndDate);
                            L1PanelCount = L1Panels.Select(x => x.PanelId).Distinct().Count();
                            L1SlotCounts = L1Panels?.Count();
                        }
                        if (slotRequirementModel.GKSlotsRequired > 0)
                        {
                            var GKPanels = panelSlots.Where(x => panels.Where(x => x.Type == "GK").Select(x => x.Id).Contains(x.PanelId) && x.SlotDate >= slotRequirementModel.StartDate && x.SlotDate <= slotRequirementModel.EndDate);
                            GKPanelCount = GKPanels.Select(x => x.PanelId).Distinct().Count();
                            GKSlotCounts = GKPanels?.Count();
                        }
                    }

                    var panelSlotsRequirement = new PanelSlotsRequirement()
                    {
                        Id = slotRequirementModel.Id,
                        TDC = slotRequirementModel.TDC,
                        CommunityId = slotRequirementModel.CommunityId,
                        SeniorityId = slotRequirementModel.SeniorityId,
                        StartDate = slotRequirementModel.StartDate.Value.Date,
                        EndDate = slotRequirementModel.EndDate.Value.Date,
                        PositionToBeFilled = slotRequirementModel.PositionToBeFilled,
                        DropRatio = slotRequirementModel.DropRatio,
                        OffersToBeRolledOut = slotRequirementModel.OffersToBeRolledOut,
                        L1SlotsRequired = slotRequirementModel.L1SlotsRequired,
                        L1SlotsActual = L1SlotCounts,
                        GKSlotsRequired = slotRequirementModel.GKSlotsRequired,
                        GKSlotsActual = GKSlotCounts,
                        L1SelectionRatio = slotRequirementModel.L1SelectionRatio,
                        GKSelectionRatio = slotRequirementModel.GKSelectionRatio,
                        L1Panels = L1PanelCount,
                        GKPanels = GKPanelCount
                    };
                    if(updateSlot)
                    {
                        panelSlotsRequirement.UpdatedBy = _authenticatedUserService.AuthUser.Id;
                        panelSlotsRequirement.UpdatedOn = DateTime.UtcNow;
                    }
                    else
                    {
                        panelSlotsRequirement.CreatedBy = _authenticatedUserService.AuthUser.Id;
                        panelSlotsRequirement.CreatedOn = DateTime.UtcNow;
                    }
                    backendModels.Add(panelSlotsRequirement);
                }

            }
            return backendModels;
        }

        public async Task<bool> CreateSlotManagement(List<SlotRequirementModel> slotRequirementModel)
        {
            foreach (var model in slotRequirementModel)
            {
                if (model.StartDate.HasValue && model.EndDate.HasValue)
                {
                    model.StartDate = TimeZoneInfo.ConvertTimeFromUtc(model.StartDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    model.EndDate = TimeZoneInfo.ConvertTimeFromUtc(model.EndDate.Value, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                }
            }


            bool response = false;
            List<PanelSlotsRequirement> panelSlotsRequirement = CreateModel(slotRequirementModel, false);

            _slotsRequirementRepository.Insert(panelSlotsRequirement);
            _unitOfWork.SaveChanges();
            response = true;

            return response;
        }

        public async Task DeleteSlotManagement(int id)
        {
            var slotReq = await _slotsRequirementRepository.GetAll().Where(x => x.Id == id).ToListAsync();
            _slotsRequirementRepository.Delete(slotReq);
            _unitOfWork.SaveChanges();
        }
    }
}
