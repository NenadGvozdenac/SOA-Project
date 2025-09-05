package models

type User struct {
	ID             uint   `gorm:"primary_key"`
	Name           string `json:"name"`
	Surname        string `json:"surname"`
	Email          string `json:"email"`
	Username       string `json:"username"`
	Password       string `json:"-"`
	Biography      string `json:"biography"`
	Moto           string `json:"motto"`
	ProfilePicture []byte `json:"profile_picture" gorm:"type:bytea"`
	RoleId         uint   `json:"role_id"`
	Blocked        bool   `json:"blocked"`
}
