#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
#import <GoogleSignIn/GoogleSignIn.h>

extern void UnitySendMessage(const char* obj, const char* method, const char* msg);

static NSString* MakeNSString(const char* value)
{
    if (value == NULL)
    {
        return @"";
    }

    return [NSString stringWithUTF8String:value] ?: @"";
}

static NSString* EscapeJson(NSString* value)
{
    if (value == nil)
    {
        return @"";
    }

    NSString* result = value;
    result = [result stringByReplacingOccurrencesOfString:@"\\" withString:@"\\\\"];
    result = [result stringByReplacingOccurrencesOfString:@"\"" withString:@"\\\""];
    result = [result stringByReplacingOccurrencesOfString:@"\n" withString:@"\\n"];
    result = [result stringByReplacingOccurrencesOfString:@"\r" withString:@"\\r"];

    return result;
}

static UIViewController* RootViewController()
{
    UIWindow* keyWindow = nil;

    if (@available(iOS 15.0, *))
    {
        for (UIScene* scene in UIApplication.sharedApplication.connectedScenes)
        {
            if (scene.activationState != UISceneActivationStateForegroundActive)
            {
                continue;
            }
            if (![scene isKindOfClass:UIWindowScene.class])
            {
                continue;
            }
            UIWindowScene* windowScene = (UIWindowScene*)scene;
            for (UIWindow* window in windowScene.windows)
            {
                if (window.isKeyWindow)
                {
                    keyWindow = window;
                    break;
                }
            }

            if (keyWindow != nil)
            {
                break;
            }
        }
    }
    if (keyWindow == nil)
    {
        keyWindow = UIApplication.sharedApplication.keyWindow;
    }
    return keyWindow.rootViewController;
}

static NSArray<NSString*>* ParseScopes(NSString* scopeText)
{
    if (scopeText.length <= 0)
    {
        return @[];
    }

    NSArray<NSString*>* parts = [scopeText componentsSeparatedByString:@" "];
    NSMutableArray<NSString*>* scopes = [NSMutableArray array];

    for (NSString* part in parts)
    {
        NSString* trimmed = [part stringByTrimmingCharactersInSet:NSCharacterSet.whitespaceAndNewlineCharacterSet];
        if (trimmed.length <= 0)
        {
            continue;
        }
        [scopes addObject:trimmed];
    }
    return scopes;
}

static void SendResult(
    NSString* objectName,
    NSString* callbackName,
    BOOL success,
    NSString* accessToken,
    NSString* tokenType,
    NSString* scope,
    long long expiresAtUnixTime,
    NSString* error
)
{
    NSString* json = [NSString stringWithFormat:
        @"{\"success\":%@,"
         "\"accessToken\":\"%@\","
         "\"tokenType\":\"%@\","
         "\"scope\":\"%@\","
         "\"expiresAtUnixTime\":%lld,"
         "\"error\":\"%@\"}",
        success ? @"true" : @"false",
        EscapeJson(accessToken),
        EscapeJson(tokenType),
        EscapeJson(scope),
        expiresAtUnixTime,
        EscapeJson(error)
    ];

    UnitySendMessage( [objectName UTF8String], [callbackName UTF8String], [json UTF8String] );
}

static void FinishWithUser(
    GIDGoogleUser* user,
    NSArray<NSString*>* scopes,
    UIViewController* presenter,
    NSString* objectName,
    NSString* callbackName
)
{
    void (^refreshBlock)(GIDGoogleUser*) = ^(GIDGoogleUser* scopedUser)
    {
        [scopedUser refreshTokensIfNeededWithCompletion: ^(GIDGoogleUser* refreshedUser, NSError* refreshError)
        {
            if (refreshError != nil)
            {
                SendResult( objectName, callbackName, NO, @"", @"", @"", 0, refreshError.localizedDescription );
                return;
            }
            GIDGoogleUser* finalUser = refreshedUser ?: scopedUser;
            NSString* accessToken = finalUser.accessToken.tokenString ?: @"";
            NSDate* expirationDate = finalUser.accessToken.expirationDate;
            long long expiresAt = expirationDate != nil
                ? (long long)[expirationDate timeIntervalSince1970]
                : (long long)( [[NSDate date] timeIntervalSince1970] + 3600 );

            if (accessToken.length <= 0)
            {
                SendResult( objectName, callbackName, NO, @"", @"", @"", 0, @"Google access token is empty." );
                return;
            }

            SendResult( objectName, callbackName, YES, accessToken, @"Bearer", [scopes componentsJoinedByString:@" "], expiresAt, @"" );
        }];
    };
    NSArray<NSString*>* grantedScopes = user.grantedScopes ?: @[];
    NSMutableArray<NSString*>* missingScopes = [NSMutableArray array];
    for (NSString* requestedScope in scopes)
    {
        if (![grantedScopes containsObject:requestedScope])
        {
            [missingScopes addObject:requestedScope];
        }
    }

    if (missingScopes.count <= 0)
    {
        refreshBlock(user);
        return;
    }
    
    [user addScopes:missingScopes presentingViewController:presenter completion: ^(GIDSignInResult* signInResult, NSError* scopeError)
    {
        if (scopeError != nil)
        {
            if (scopeError.code == kGIDSignInErrorCodeScopesAlreadyGranted)
            {
                refreshBlock(user);
                return;
            }
            SendResult( objectName, callbackName, NO, @"", @"", @"", 0, scopeError.localizedDescription );
            return;
        }
        GIDGoogleUser* scopedUser = signInResult.user ?: user;
        refreshBlock(scopedUser);
    }];
}

extern "C" void GoogleAuth_RequestAccessToken(
    const char* iosClientId,
    const char* unityGameObjectName,
    const char* unityCallbackMethodName,
    const char* scopeText
)
{
    NSString* clientId = MakeNSString(iosClientId);
    NSString* objectName = MakeNSString(unityGameObjectName);
    NSString* callbackName = MakeNSString(unityCallbackMethodName);
    NSString* scopeString = MakeNSString(scopeText);
    
    if (clientId.length <= 0)
    {
        SendResult( objectName, callbackName, NO, @"", @"", @"", 0, @"Google OAuth iOS client ID is empty." );
        return;
    }
    NSArray<NSString*>* scopes = ParseScopes(scopeString);
    dispatch_async(dispatch_get_main_queue(), ^
    {
        GIDConfiguration* configuration = [[GIDConfiguration alloc] initWithClientID:clientId];
        [GIDSignIn sharedInstance].configuration = configuration;
        UIViewController* presenter = RootViewController();
        if (presenter == nil)
        {
            SendResult( objectName, callbackName, NO, @"", @"", @"", 0, @"Root view controller not found." );
            return;
        }

        [[GIDSignIn sharedInstance] restorePreviousSignInWithCompletion: ^(GIDGoogleUser* restoredUser, NSError* restoreError)
        {
            if (restoredUser != nil)
            {
                FinishWithUser( restoredUser, scopes, presenter, objectName, callbackName );
                return;
            }

            [[GIDSignIn sharedInstance] signInWithPresentingViewController:presenter completion: ^(GIDSignInResult* signInResult, NSError* signInError)
            {
                if (signInError != nil)
                {
                    SendResult( objectName, callbackName, NO, @"", @"", @"", 0, signInError.localizedDescription );
                    return;
                }
                if (signInResult.user == nil)
                {
                    SendResult( objectName, callbackName, NO, @"", @"", @"", 0, @"Google user is null." );
                    return;
                }
                FinishWithUser( signInResult.user, scopes, presenter, objectName, callbackName );
            }];
        }];
    });
}

extern "C" void GoogleAuth_SignOut()
{
    dispatch_async(dispatch_get_main_queue(), ^
    {
        [[GIDSignIn sharedInstance] signOut];
    });
}