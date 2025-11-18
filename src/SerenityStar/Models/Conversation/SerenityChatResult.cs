using System.Text.Json.Serialization;

namespace SerenityStar.Models.Conversation
{
    /// <summary>
    /// Represents SerenityChat channel configuration.
    /// </summary>
    public class SerenityChatResult
    {
        /// <summary>
        /// Indicates if the chat widget is expanded.
        /// </summary>
        [JsonPropertyName("expanded")]
        public bool Expanded { get; set; }

        /// <summary>
        /// Indicates if the chat widget is opened.
        /// </summary>
        [JsonPropertyName("opened")]
        public bool Opened { get; set; }

        /// <summary>
        /// Indicates if the expand button should be shown.
        /// </summary>
        [JsonPropertyName("showExpandButton")]
        public bool ShowExpandButton { get; set; }

        /// <summary>
        /// Indicates if file upload is allowed.
        /// </summary>
        [JsonPropertyName("allowUpload")]
        public bool AllowUpload { get; set; }

        /// <summary>
        /// Indicates if audio recording is allowed.
        /// </summary>
        [JsonPropertyName("allowAudioRecording")]
        public bool AllowAudioRecording { get; set; }

        /// <summary>
        /// Indicates if conversations should be stored.
        /// </summary>
        [JsonPropertyName("storeConversation")]
        public bool StoreConversation { get; set; }

        /// <summary>
        /// Indicates if streaming is enabled.
        /// </summary>
        [JsonPropertyName("stream")]
        public bool Stream { get; set; }

        /// <summary>
        /// Indicates if page content extraction is enabled.
        /// </summary>
        [JsonPropertyName("extractPageContent")]
        public bool ExtractPageContent { get; set; }

        /// <summary>
        /// Indicates if the system logo should be shown.
        /// </summary>
        [JsonPropertyName("showSystemLogo")]
        public bool ShowSystemLogo { get; set; }

        /// <summary>
        /// The user identifier.
        /// </summary>
        [JsonPropertyName("userIdentifier")]
        public string UserIdentifier { get; set; } = string.Empty;

        /// <summary>
        /// The logo URL.
        /// </summary>
        [JsonPropertyName("logoURL")]
        public string? LogoURL { get; set; }

        /// <summary>
        /// The presentation mode.
        /// </summary>
        [JsonPropertyName("mode")]
        public string Mode { get; set; } = string.Empty;

        /// <summary>
        /// The engagement message configuration.
        /// </summary>
        [JsonPropertyName("engagementMessage")]
        public EngagementMessageRes? EngagementMessage { get; set; }

        /// <summary>
        /// The localization configuration.
        /// </summary>
        [JsonPropertyName("locale")]
        public LocaleRes? Locale { get; set; }

        /// <summary>
        /// The theme configuration.
        /// </summary>
        [JsonPropertyName("theme")]
        public ThemeRes? Theme { get; set; }

        /// <summary>
        /// Indicates if feedback recollection is enabled.
        /// </summary>
        [JsonPropertyName("enableFeedbackRecollection")]
        public bool EnableFeedbackRecollection { get; set; }

        /// <summary>
        /// Represents engagement message configuration.
        /// </summary>
        public class EngagementMessageRes
        {
            /// <summary>
            /// Indicates if the engagement message is enabled.
            /// </summary>
            [JsonPropertyName("enabled")]
            public bool Enabled { get; set; }

            /// <summary>
            /// The engagement message text.
            /// </summary>
            [JsonPropertyName("message")]
            public string Message { get; set; } = string.Empty;

            /// <summary>
            /// The delay in seconds before showing the engagement message.
            /// </summary>
            [JsonPropertyName("showAfter")]
            public int ShowAfter { get; set; }
        }

        /// <summary>
        /// Represents localization settings.
        /// </summary>
        public class LocaleRes
        {
            /// <summary>
            /// Error message for file upload failure.
            /// </summary>
            [JsonPropertyName("uploadFileErrorMessage")]
            public string UploadFileErrorMessage { get; set; } = string.Empty;

            /// <summary>
            /// Error message for multiple files upload failure.
            /// </summary>
            [JsonPropertyName("uploadFilesErrorMessage")]
            public string UploadFilesErrorMessage { get; set; } = string.Empty;

            /// <summary>
            /// General chat error message.
            /// </summary>
            [JsonPropertyName("chatErrorMessage")]
            public string ChatErrorMessage { get; set; } = string.Empty;

            /// <summary>
            /// Error message for conversation initialization failure.
            /// </summary>
            [JsonPropertyName("chatInitConversationErrorMessage")]
            public string ChatInitConversationErrorMessage { get; set; } = string.Empty;

            /// <summary>
            /// The header title text.
            /// </summary>
            [JsonPropertyName("headerTitle")]
            public string HeaderTitle { get; set; } = string.Empty;

            /// <summary>
            /// Message shown when conversation is finalized.
            /// </summary>
            [JsonPropertyName("finalizedMessage")]
            public string FinalizedMessage { get; set; } = string.Empty;

            /// <summary>
            /// Message shown when response limit is exceeded.
            /// </summary>
            [JsonPropertyName("limitExceededMessage")]
            public string LimitExceededMessage { get; set; } = string.Empty;

            /// <summary>
            /// Label for wait until message.
            /// </summary>
            [JsonPropertyName("waitUntilMessage")]
            public string WaitUntilMessage { get; set; } = string.Empty;

            /// <summary>
            /// Label for remaining time message.
            /// </summary>
            [JsonPropertyName("remainingMessage")]
            public string RemainingMessage { get; set; } = string.Empty;

            /// <summary>
            /// Placeholder text for input field.
            /// </summary>
            [JsonPropertyName("inputPlaceholder")]
            public string InputPlaceholder { get; set; } = string.Empty;

            /// <summary>
            /// Message for internal links.
            /// </summary>
            [JsonPropertyName("internalLinkMessage")]
            public string InternalLinkMessage { get; set; } = string.Empty;

            /// <summary>
            /// Text for new chat button.
            /// </summary>
            [JsonPropertyName("newChatBtnMessage")]
            public string NewChatBtnMessage { get; set; } = string.Empty;

            /// <summary>
            /// Error message when maximum number of files is exceeded.
            /// </summary>
            [JsonPropertyName("exceededMaxNumberOfFilesMessage")]
            public string ExceededMaxNumberOfFilesMessage { get; set; } = string.Empty;

            /// <summary>
            /// Error message when file upload takes too long.
            /// </summary>
            [JsonPropertyName("exceededMaxFileStatusChecksMessage")]
            public string ExceededMaxFileStatusChecksMessage { get; set; } = string.Empty;

            /// <summary>
            /// Message shown when conversation is closed.
            /// </summary>
            [JsonPropertyName("closedConversationMessage")]
            public string ClosedConversationMessage { get; set; } = string.Empty;

            /// <summary>
            /// Label for connect button.
            /// </summary>
            [JsonPropertyName("connectButtonLabel")]
            public string ConnectButtonLabel { get; set; } = string.Empty;

            /// <summary>
            /// Message shown when a single connection is required.
            /// </summary>
            [JsonPropertyName("connectionRequiredSingularMessage")]
            public string ConnectionRequiredSingularMessage { get; set; } = string.Empty;

            /// <summary>
            /// Message shown when multiple connections are required.
            /// </summary>
            [JsonPropertyName("connectionRequiredPluralMessage")]
            public string ConnectionRequiredPluralMessage { get; set; } = string.Empty;
        }

        /// <summary>
        /// Represents theme configuration.
        /// </summary>
        public class ThemeRes
        {
            /// <summary>
            /// Header theme configuration.
            /// </summary>
            [JsonPropertyName("header")]
            public HeaderThemeRes? Header { get; set; }

            /// <summary>
            /// Floating action button theme configuration.
            /// </summary>
            [JsonPropertyName("fabButton")]
            public FabButtonThemeRes? FabButton { get; set; }

            /// <summary>
            /// Send button theme configuration.
            /// </summary>
            [JsonPropertyName("sendButton")]
            public SendButtonThemeRes? SendButton { get; set; }

            /// <summary>
            /// Engagement message theme configuration.
            /// </summary>
            [JsonPropertyName("engagementMessage")]
            public EngagementMessageThemeRes? EngagementMessage { get; set; }

            /// <summary>
            /// Conversation starters theme configuration.
            /// </summary>
            [JsonPropertyName("conversationStarters")]
            public ConversationStartersThemeRes? ConversationStarters { get; set; }

            /// <summary>
            /// Scroll to bottom indicator theme configuration.
            /// </summary>
            [JsonPropertyName("scrollToBottomIndicator")]
            public ScrollToBottomIndicatorThemeRes? ScrollToBottomIndicator { get; set; }

            /// <summary>
            /// Upload file button theme configuration.
            /// </summary>
            [JsonPropertyName("uploadFileBtn")]
            public UploadFileBtnThemeRes? UploadFileBtn { get; set; }

            /// <summary>
            /// Message bubble theme configuration.
            /// </summary>
            [JsonPropertyName("messageBubble")]
            public MessageBubbleThemeRes? MessageBubble { get; set; }

            /// <summary>
            /// Represents header theme configuration.
            /// </summary>
            public class HeaderThemeRes
            {
                /// <summary>
                /// Background color.
                /// </summary>
                [JsonPropertyName("bgColor")]
                public string BgColor { get; set; } = string.Empty;

                /// <summary>
                /// Text color.
                /// </summary>
                [JsonPropertyName("textColor")]
                public string TextColor { get; set; } = string.Empty;

                /// <summary>
                /// Reset chat button theme configuration.
                /// </summary>
                [JsonPropertyName("resetChatBtn")]
                public ResetChatBtnThemeRes? ResetChatBtn { get; set; }

                /// <summary>
                /// Minimize button theme configuration.
                /// </summary>
                [JsonPropertyName("minimizeBtn")]
                public MinimizeBtnThemeRes? MinimizeBtn { get; set; }

                /// <summary>
                /// Represents minimize button theme configuration.
                /// </summary>
                public class MinimizeBtnThemeRes
                {
                    /// <summary>
                    /// Icon stroke color.
                    /// </summary>
                    [JsonPropertyName("iconStrokeColor")]
                    public string IconStrokeColor { get; set; } = string.Empty;
                }

                /// <summary>
                /// Represents reset chat button theme configuration.
                /// </summary>
                public class ResetChatBtnThemeRes
                {
                    /// <summary>
                    /// Background color.
                    /// </summary>
                    [JsonPropertyName("bgColor")]
                    public string BgColor { get; set; } = string.Empty;

                    /// <summary>
                    /// Hover background color.
                    /// </summary>
                    [JsonPropertyName("hoverBgColor")]
                    public string HoverBgColor { get; set; } = string.Empty;

                    /// <summary>
                    /// Text color.
                    /// </summary>
                    [JsonPropertyName("textColor")]
                    public string TextColor { get; set; } = string.Empty;
                }
            }

            /// <summary>
            /// Represents floating action button theme configuration.
            /// </summary>
            public class FabButtonThemeRes
            {
                /// <summary>
                /// Background color.
                /// </summary>
                [JsonPropertyName("bgColor")]
                public string BgColor { get; set; } = string.Empty;

                /// <summary>
                /// Icon stroke color.
                /// </summary>
                [JsonPropertyName("iconStrokeColor")]
                public string IconStrokeColor { get; set; } = string.Empty;

                /// <summary>
                /// Button size in pixels.
                /// </summary>
                [JsonPropertyName("buttonSize")]
                public int ButtonSize { get; set; }

                /// <summary>
                /// Icon size in pixels.
                /// </summary>
                [JsonPropertyName("iconSize")]
                public int IconSize { get; set; }
            }

            /// <summary>
            /// Represents send button theme configuration.
            /// </summary>
            public class SendButtonThemeRes
            {
                /// <summary>
                /// Background color.
                /// </summary>
                [JsonPropertyName("bgColor")]
                public string BgColor { get; set; } = string.Empty;

                /// <summary>
                /// Icon color.
                /// </summary>
                [JsonPropertyName("iconStrokeColor")]
                public string IconColor { get; set; } = string.Empty;
            }

            /// <summary>
            /// Represents engagement message theme configuration.
            /// </summary>
            public class EngagementMessageThemeRes
            {
                /// <summary>
                /// Background color.
                /// </summary>
                [JsonPropertyName("bgColor")]
                public string BgColor { get; set; } = string.Empty;

                /// <summary>
                /// Text color.
                /// </summary>
                [JsonPropertyName("textColor")]
                public string TextColor { get; set; } = string.Empty;
            }

            /// <summary>
            /// Represents conversation starters theme configuration.
            /// </summary>
            public class ConversationStartersThemeRes
            {
                /// <summary>
                /// Background color.
                /// </summary>
                [JsonPropertyName("bgColor")]
                public string BgColor { get; set; } = string.Empty;

                /// <summary>
                /// Text color.
                /// </summary>
                [JsonPropertyName("textColor")]
                public string TextColor { get; set; } = string.Empty;

                /// <summary>
                /// Container background color.
                /// </summary>
                [JsonPropertyName("containerBgColor")]
                public string ContainerBgColor { get; set; } = string.Empty;

                /// <summary>
                /// Initial message background color.
                /// </summary>
                [JsonPropertyName("initialMessageBgColor")]
                public string InitialMessageBgColor { get; set; } = string.Empty;

                /// <summary>
                /// Initial message text color.
                /// </summary>
                [JsonPropertyName("initialMessageTextColor")]
                public string InitialMessageTextColor { get; set; } = string.Empty;
            }

            /// <summary>
            /// Represents scroll to bottom indicator theme configuration.
            /// </summary>
            public class ScrollToBottomIndicatorThemeRes
            {
                /// <summary>
                /// Background color.
                /// </summary>
                [JsonPropertyName("bgColor")]
                public string BgColor { get; set; } = string.Empty;

                /// <summary>
                /// Icon stroke color.
                /// </summary>
                [JsonPropertyName("iconStrokeColor")]
                public string IconStrokeColor { get; set; } = string.Empty;
            }

            /// <summary>
            /// Represents upload file button theme configuration.
            /// </summary>
            public class UploadFileBtnThemeRes
            {
                /// <summary>
                /// Icon stroke color.
                /// </summary>
                [JsonPropertyName("iconStrokeColor")]
                public string IconStrokeColor { get; set; } = string.Empty;
            }

            /// <summary>
            /// Represents message bubble theme configuration.
            /// </summary>
            public class MessageBubbleThemeRes
            {
                /// <summary>
                /// User message theme configuration.
                /// </summary>
                [JsonPropertyName("user")]
                public UserThemeRes? User { get; set; }

                /// <summary>
                /// Assistant message theme configuration.
                /// </summary>
                [JsonPropertyName("assistant")]
                public AssistantThemeRes? Assistant { get; set; }

                /// <summary>
                /// Represents user message theme configuration.
                /// </summary>
                public class UserThemeRes
                {
                    /// <summary>
                    /// Background color.
                    /// </summary>
                    [JsonPropertyName("bgColor")]
                    public string BgColor { get; set; } = string.Empty;

                    /// <summary>
                    /// Text color.
                    /// </summary>
                    [JsonPropertyName("textColor")]
                    public string TextColor { get; set; } = string.Empty;
                }

                /// <summary>
                /// Represents assistant message theme configuration.
                /// </summary>
                public class AssistantThemeRes
                {
                    /// <summary>
                    /// Background color.
                    /// </summary>
                    [JsonPropertyName("bgColor")]
                    public string BgColor { get; set; } = string.Empty;

                    /// <summary>
                    /// Text color.
                    /// </summary>
                    [JsonPropertyName("textColor")]
                    public string TextColor { get; set; } = string.Empty;

                    /// <summary>
                    /// Connections theme configuration.
                    /// </summary>
                    [JsonPropertyName("connections")]
                    public ConnectionsThemeRes? Connections { get; set; }

                    /// <summary>
                    /// Represents connections theme configuration.
                    /// </summary>
                    public class ConnectionsThemeRes
                    {
                        /// <summary>
                        /// Connection card theme configuration.
                        /// </summary>
                        [JsonPropertyName("connectionCard")]
                        public ConnectionCardThemeRes? ConnectionCard { get; set; }

                        /// <summary>
                        /// Represents connection card theme configuration.
                        /// </summary>
                        public class ConnectionCardThemeRes
                        {
                            /// <summary>
                            /// Background color.
                            /// </summary>
                            [JsonPropertyName("bgColor")]
                            public string BgColor { get; set; } = string.Empty;

                            /// <summary>
                            /// Text color.
                            /// </summary>
                            [JsonPropertyName("textColor")]
                            public string TextColor { get; set; } = string.Empty;

                            /// <summary>
                            /// Connect button theme configuration.
                            /// </summary>
                            [JsonPropertyName("connectButton")]
                            public ConnectButtonThemeRes? ConnectButton { get; set; }

                            /// <summary>
                            /// Represents connect button theme configuration.
                            /// </summary>
                            public class ConnectButtonThemeRes
                            {
                                /// <summary>
                                /// Background color.
                                /// </summary>
                                [JsonPropertyName("bgColor")]
                                public string BgColor { get; set; } = string.Empty;

                                /// <summary>
                                /// Text color.
                                /// </summary>
                                [JsonPropertyName("textColor")]
                                public string TextColor { get; set; } = string.Empty;
                            }
                        }
                    }
                }
            }
        }
    }
}
