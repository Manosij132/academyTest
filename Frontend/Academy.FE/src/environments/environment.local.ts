// this config file is design to work with local docker env.

export const environment = {
    production: false,
    apiExtension: "api",
    apiMockinterviewBaseURL: "https://nce6822dxu06.tat.corp.globant.com:8080",
    GoogleProviderClientId: "608246599648-iv23qrod1c3gn48bsbleinikmfbtc7v6.apps.googleusercontent.com",
    apiBaseUrl: 'https://localhost',
    speedRanges: [
        { min: 0, max: 25, label: "Cannot perform" },
        { min: 25, max: 50, label: "Beginner" },
        { min: 25, max: 75, label: "Intermediate" },
        { min: 75, max: 100, label: "Advance" }
    ],
    academyBaseUrl: "https://localhost/",
    emailBaseUrl: "https://nce6822dxu06.tat.corp.globant.com:8443",
    staffingApiBaseurl: 'https://localhost/staffing/api'
};

// linux server -QA db
// With docker env setup
// export const environment = {
//   production: false,
//    apiExtension: "api",
//    apiBaseUrl: "https://nce6822dxu06.tat.corp.globant.com:9002/api",
//         apiMockinterviewBaseURL: "https://nce6822dxu06.tat.corp.globant.com:8080",
//         GoogleProviderClientId: "608246599648-iv23qrod1c3gn48bsbleinikmfbtc7v6.apps.googleusercontent.com",
//         speedRanges: [
//             {min: 0,max: 25,label: "Cannot perform"
//             },
//             {min: 25,max: 50,label: "Beginner"
//             },
//             {min: 25,max: 75,label: "Intermediate"
//             },
//             {min: 75,max: 100,label: "Advance"
//             }
//         ],
//         academyBaseUrl: "https://nce6822dxu06.tat.corp.globant.com:9002/",
//         emailBaseUrl: "https://nce6822dxu06.tat.corp.globant.com:8443",
//         staffingApiBaseurl: "https://nce6822dxu06.tat.corp.globant.com:9002/staffing/api"
// };