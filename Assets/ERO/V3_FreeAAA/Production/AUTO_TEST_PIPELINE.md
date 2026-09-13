# ERO Automatic Unity Test Pipeline

This project now contains a GitHub Actions pipeline intended to:
1. open the project with Unity 6000.0.67f1,
2. run Unity tests,
3. build a validation player,
4. upload logs/results as workflow artifacts.

Important:
- GitHub/Unity account authorization is intentionally NOT embedded in the project.
- Add `UNITY_EMAIL` and `UNITY_PASSWORD` as GitHub repository Actions secrets.
- Never commit credentials or Unity tokens into the repository.
- If the exact 6000.0.67f1 editor image is unavailable in the chosen runner, switch the workflow to the closest supported Unity 6000.0 LTS image or use Unity Build Automation.
