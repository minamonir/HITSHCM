namespace Hitshcm.Web.Login;

/// <summary>
/// CURRENT <c>CheckURules</c> port. Fail-<b>closed</b> on error (unlike live DNA, which defaulted to 001).
/// </summary>
public interface ILoginPolicyEvaluator
{
    LoginPolicyResult Evaluate(LoginPolicyInput? input);
}
