# Changelog

All notable changes to the QuartzHangfire.Extensions project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.2] - 2025-12-09

### Added
- Overloaded `ContinueJobWith` methods supporting string keys (in addition to JobKey)
- Added release notes (CHANGELOG.md) to the NuGet package
- Included CHANGELOG.md in the packaged files

## [1.0.1] - 2025-12-09

### Added
- Improved job chaining documentation and examples

### Changed
- Updated minimum dependency version requirements
- Enhanced README with clearer examples for job continuation

### Fixed
- Scheduler job enqueuing and async method handling
- Performance improvements for job scheduling reflection

## [1.0.0] - 2025-12-06

### Added
- Initial release of QuartzHangfire.Extensions
- Hangfire-like syntax for Quartz.NET job scheduling
- Support for immediate job execution with `Enqueue` methods
- Support for delayed job execution with `Schedule` methods
- Job continuation support with `ContinueJobWith` methods
- Queue support for job prioritization
- Generic method support for both static and instance methods
- Full async/await support for asynchronous job processing
- Multi-target support for .NET 8.0, 9.0, and 10.0