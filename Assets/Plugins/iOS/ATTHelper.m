#import <AppTrackingTransparency/AppTrackingTransparency.h>

// Callback type matching Unity's delegate
typedef void (*ATTCompletionCallback)(int status);

void ATTHelper_RequestPermission(ATTCompletionCallback callback)
{
    if (@available(iOS 14, *))
    {
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            dispatch_async(dispatch_get_main_queue(), ^{
                if (callback != NULL)
                {
                    callback((int)status);
                }
            });
        }];
    }
    else
    {
        // iOS < 14: tracking izni gerekmez, authorized donelim
        if (callback != NULL)
        {
            callback(3); // ATTrackingManagerAuthorizationStatusAuthorized
        }
    }
}
