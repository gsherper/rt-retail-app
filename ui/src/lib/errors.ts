/* 

  Message to show the user for a failed hub call: the first field error,
  else the server's detail, else defaultMessage.

  SignalR Error message sample

{ "type": 3, 
     "invocationId": "3",
      "result": { 
        "ok": false, 
        "data": null,
        "error": { 
          "status": 404, "title": "Not Found", 
          "detail": "The customer was not found.", 
          "errors": null 
        } 
      }
    }
*/

export function errorMessage(error: unknown, defaultMessage: string) {
  const data = (
    error as {
      data?: {
        detail?: string | null
        errors?: Record<string, string[]> | null
      }
    }
  )?.data
  const firstFieldError = data?.errors
    ? Object.values(data.errors)[0]?.[0]
    : undefined
  return firstFieldError ?? data?.detail ?? defaultMessage
}
