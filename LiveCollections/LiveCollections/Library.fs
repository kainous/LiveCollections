namespace System.Collections.LiveCollections

type Notification<'TIndex, 'TValue> =
| Addition of 'TIndex * 'TValue
| Deletion of 'TIndex