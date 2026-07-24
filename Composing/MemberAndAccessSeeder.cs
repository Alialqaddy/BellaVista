using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace BellaVista.Composing;

/// <summary>
/// Chapter 8 "Der Membersbereich": a member group for loyal guests and one test member,
/// using Umbraco's built-in Member system (the default "Member" member type - we don't
/// need a custom one, the loyal-guests page just needs *any* logged-in member from that group).
/// </summary>
public class MemberAndAccessSeeder
{
    private readonly IMemberService _memberService;
    private readonly IMemberManager _memberManager;
    private readonly IMemberGroupService _memberGroupService;
    private readonly IPublicAccessService _publicAccessService;

    public const string LoyalGuestsGroupName = "Loyal Guests";
    public const string TestMemberEmail = "guest@bellavista.local";
    public const string TestMemberPassword = "LoyalGuest2026!";

    public MemberAndAccessSeeder(
        IMemberService memberService,
        IMemberManager memberManager,
        IMemberGroupService memberGroupService,
        IPublicAccessService publicAccessService)
    {
        _memberService = memberService;
        _memberManager = memberManager;
        _memberGroupService = memberGroupService;
        _publicAccessService = publicAccessService;
    }

    /// <summary>
    /// Creates the member through IMemberManager (not the older IMemberService) so the
    /// password is hashed the same way ASP.NET Core Identity's sign-in later expects it -
    /// IMemberService.CreateWithIdentity stores the raw value as-is and login then fails.
    /// </summary>
    public async Task SeedGroupAndTestMemberAsync()
    {
        if (_memberGroupService.GetByName(LoyalGuestsGroupName) == null)
        {
            _memberGroupService.Save(new MemberGroup { Name = LoyalGuestsGroupName });
        }

        if (_memberService.GetByEmail(TestMemberEmail) == null)
        {
            MemberIdentityUser user = MemberIdentityUser.CreateNew(
                TestMemberEmail,
                TestMemberEmail,
                Constants.Conventions.MemberTypes.DefaultAlias,
                isApproved: true,
                name: "Test Loyal Guest");

            await _memberManager.CreateAsync(user, TestMemberPassword);

            // Role assignment isn't password-related, so the older IMemberService is fine here.
            IMember? createdMember = _memberService.GetByEmail(TestMemberEmail);
            if (createdMember != null)
            {
                _memberService.AssignRoles(new[] { createdMember.Id }, new[] { LoyalGuestsGroupName });
            }
        }
    }

    /// <summary>Restricts a content node (the "Loyal Guests" page) to members of the loyal-guests group.</summary>
    public void ProtectNode(IContent protectedNode, IContent loginNode, IContent errorNode)
    {
        if (_publicAccessService.IsProtected(protectedNode).Success) return;

        var entry = new PublicAccessEntry(protectedNode, loginNode, errorNode, Array.Empty<PublicAccessRule>());
        entry.AddRule(LoyalGuestsGroupName, Constants.Conventions.PublicAccess.MemberRoleRuleType);
        _publicAccessService.Save(entry);
    }
}
