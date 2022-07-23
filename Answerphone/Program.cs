using Answerphone;

using AnswerphoneController controller = new("COM9", "AudioFiles\\Greetings", "AudioFiles\\FollowUp");
controller.Run();