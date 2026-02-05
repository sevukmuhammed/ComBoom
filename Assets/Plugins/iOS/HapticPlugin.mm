#import <UIKit/UIKit.h>

extern "C" {
    void _TriggerImpactHaptic(int style) {
        if (@available(iOS 10.0, *)) {
            UIImpactFeedbackStyle feedbackStyle;
            switch (style) {
                case 0: feedbackStyle = UIImpactFeedbackStyleLight; break;
                case 1: feedbackStyle = UIImpactFeedbackStyleMedium; break;
                case 2: feedbackStyle = UIImpactFeedbackStyleHeavy; break;
                default: feedbackStyle = UIImpactFeedbackStyleMedium; break;
            }
            UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:feedbackStyle];
            [generator prepare];
            [generator impactOccurred];
        }
    }

    void _TriggerNotificationHaptic(int type) {
        if (@available(iOS 10.0, *)) {
            UINotificationFeedbackType feedbackType;
            switch (type) {
                case 0: feedbackType = UINotificationFeedbackTypeSuccess; break;
                case 1: feedbackType = UINotificationFeedbackTypeWarning; break;
                case 2: feedbackType = UINotificationFeedbackTypeError; break;
                default: feedbackType = UINotificationFeedbackTypeSuccess; break;
            }
            UINotificationFeedbackGenerator *generator = [UINotificationFeedbackGenerator new];
            [generator prepare];
            [generator notificationOccurred:feedbackType];
        }
    }

    void _TriggerSelectionHaptic() {
        if (@available(iOS 10.0, *)) {
            UISelectionFeedbackGenerator *generator = [UISelectionFeedbackGenerator new];
            [generator prepare];
            [generator selectionChanged];
        }
    }
}
